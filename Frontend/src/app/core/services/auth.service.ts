import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { SharedListDataService } from './shared-list-data.service';
import { DashboardService } from './dashboard.service';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '@shared/models/user.model';

// The access token is kept in-memory only so XSS cannot read it via storage APIs.
// TOKEN_FOR_REFRESH holds the raw JWT solely for the /auth/refresh-token call; it is NOT
// used for validity checks (getAccessToken() uses the in-memory value). This key survives
// a page reload so the token interceptor can perform a silent refresh on the first 401.
const TOKEN_FOR_REFRESH_KEY = 'auth_token_for_refresh';
const REFRESH_TOKEN_KEY = 'auth_refresh_token';
const USER_KEY = 'auth_user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private accessToken: string | null = null;
  private tokenExpiry: number | null = null;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private injector: Injector
  ) {
    // Restore session from localStorage on app init (e.g. after refresh)
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.apiService.post<LoginResponse>('auth/login', credentials).pipe(
      tap(response => {
        this.setAuthData(response);
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  register(data: RegisterRequest): Observable<any> {
    return this.apiService.post('auth/register', data);
  }

  refreshToken(): Observable<LoginResponse> {
    const stored = this.getStoredTokensForRefresh();
    if (!stored) {
      return throwError(() => new Error('No refresh token available'));
    }
    return this.apiService.post<LoginResponse>('auth/refresh-token', {
      token: stored.token,
      refreshToken: stored.refreshToken
    }).pipe(
      tap(response => {
        this.setAuthData(response);
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  /** Returns tokens needed for a refresh attempt. The raw JWT is read from sessionStorage so it
   *  survives a page reload even though the in-memory copy is gone. */
  getStoredTokensForRefresh(): { token: string; refreshToken: string } | null {
    const token = sessionStorage.getItem(TOKEN_FOR_REFRESH_KEY);
    const refreshToken = sessionStorage.getItem(REFRESH_TOKEN_KEY);
    if (token && refreshToken) {
      return { token, refreshToken };
    }
    return null;
  }

  logout(): void {
    this.injector.get(SharedListDataService).clearAll();
    try {
      this.injector.get(DashboardService).invalidateDashboardCache();
    } catch {
      /* avoid hard failure if DI graph changes */
    }
    this.accessToken = null;
    this.tokenExpiry = null;
    this.currentUserSubject.next(null);
    sessionStorage.removeItem(TOKEN_FOR_REFRESH_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    // Also clear any tokens that may have been written by an older version of the app.
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_refresh_token');
    localStorage.removeItem('auth_expires_at');
    localStorage.removeItem('auth_user');
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    if (this.accessToken && this.tokenExpiry && Date.now() < this.tokenExpiry) {
      return this.accessToken;
    }
    return null;
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    const user = this.currentUserSubject.value;
    const hasRefreshToken = !!sessionStorage.getItem(REFRESH_TOKEN_KEY);
    return (!!token && !!user) || (!!user && hasRefreshToken);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    const userRole = user.roleName || user.role;
    return userRole === role;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    const userRole = user.roleName || user.role;
    return userRole ? roles.includes(userRole) : false;
  }

  changePassword(data: { currentPassword: string; newPassword: string; confirmPassword: string }): Observable<any> {
    return this.apiService.post('auth/change-password', data);
  }

  forgotPassword(email: string): Observable<any> {
    return this.apiService.post('auth/forgot-password', { email });
  }

  resetPasswordWithToken(data: { email: string; token: string; newPassword: string; confirmPassword: string }): Observable<any> {
    return this.apiService.post('auth/reset-password-with-token', data);
  }

  resetUserPassword(userId: string, newPassword: string): Observable<any> {
    return this.apiService.post('auth/reset-password', { userId, newPassword });
  }

  private setAuthData(response: LoginResponse): void {
    // Keep access token in-memory only — not accessible to XSS via storage APIs.
    this.accessToken = response.token;
    this.tokenExpiry = new Date(response.expiresAt).getTime();

    const user: User = {
      ...response.user,
      role: (response.user as any).roleName || (response.user as any).role || 'Client'
    };

    // Store raw JWT for the refresh endpoint only — survives page reload so a silent refresh
    // can occur on the first 401 after reload. Not used for validity checks.
    sessionStorage.setItem(TOKEN_FOR_REFRESH_KEY, response.token);
    // Refresh token and user profile go to sessionStorage (tab-scoped, cleared on tab close).
    if (response.refreshToken) {
      sessionStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    }
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private clearStoredAuth(): void {
    this.accessToken = null;
    this.tokenExpiry = null;
    this.currentUserSubject.next(null);
    sessionStorage.removeItem(TOKEN_FOR_REFRESH_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
  }

  private loadUserFromStorage(): void {
    // On page reload the in-memory access token is gone. Restore the user profile from
    // sessionStorage so the AuthGuard doesn't immediately redirect to login; the token
    // interceptor will trigger a silent refresh on the first 401 if the refresh token is present.
    const refreshToken = sessionStorage.getItem(REFRESH_TOKEN_KEY);
    const userStr = sessionStorage.getItem(USER_KEY);

    // Also migrate any tokens left in localStorage by an older version of the app.
    this.migrateLegacyLocalStorage();

    if (!userStr || !refreshToken) {
      this.clearStoredAuth();
      return;
    }

    try {
      const userData = JSON.parse(userStr);
      const user: User = {
        ...userData,
        role: userData.role || userData.roleName || 'Client'
      };
      // Access token is gone after reload — interceptor will refresh on first 401.
      this.currentUserSubject.next(user);
    } catch {
      this.clearStoredAuth();
    }
  }

  private migrateLegacyLocalStorage(): void {
    ['auth_token', 'auth_token_for_refresh', 'auth_refresh_token', 'auth_expires_at', 'auth_user']
      .forEach(k => localStorage.removeItem(k));
  }
}
