import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { SharedListDataService } from './shared-list-data.service';
import { DashboardService } from './dashboard.service';
import { LoginRequest, LoginResponse, RegisterRequest, User, UserRole } from '@shared/models/user.model';
import { environment } from '@environments/environment';

const useCookieAuth = !!(environment as { useCookieAuth?: boolean }).useCookieAuth;

const TOKEN_KEY = 'auth_token';
const REFRESH_TOKEN_KEY = 'auth_refresh_token';
const EXPIRES_AT_KEY = 'auth_expires_at';
const USER_KEY = 'auth_user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  // Token in memory for fast access; persisted in localStorage for refresh
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
    let body: { token?: string; refreshToken?: string } = {};
    if (!useCookieAuth) {
      const stored = this.getStoredTokensForRefresh();
      if (!stored) {
        return throwError(() => new Error('No refresh token available'));
      }
      body = { token: stored.token, refreshToken: stored.refreshToken };
    }

    return this.apiService.post<LoginResponse>('auth/refresh-token', body).pipe(
      tap(response => {
        this.setAuthData(response);
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  /** Returns stored token + refreshToken for refresh attempt (even when access token expired) */
  getStoredTokensForRefresh(): { token: string; refreshToken: string } | null {
    const token = localStorage.getItem(TOKEN_KEY);
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (token && refreshToken) {
      return { token, refreshToken };
    }
    return null;
  }

  logout(): void {
    const finish = () => {
      this.injector.get(SharedListDataService).clearAll();
      try {
        this.injector.get(DashboardService).invalidateDashboardCache();
      } catch {
        /* avoid hard failure if DI graph changes */
      }
      this.clearStoredAuth();
      this.router.navigate(['/login']);
    };

    if (useCookieAuth) {
      this.apiService.post('auth/logout', {}).subscribe({
        next: () => finish(),
        error: () => finish()
      });
      return;
    }

    finish();
  }

  getAccessToken(): string | null {
    if (useCookieAuth) {
      return null;
    }
    // Use in-memory token if available and not expired
    if (this.accessToken && this.tokenExpiry && Date.now() < this.tokenExpiry) {
      return this.accessToken;
    }
    // Fall back to localStorage (e.g. after refresh)
    const storedToken = localStorage.getItem(TOKEN_KEY);
    const storedExpiry = localStorage.getItem(EXPIRES_AT_KEY);
    if (storedToken && storedExpiry) {
      const expiry = parseInt(storedExpiry, 10);
      if (Date.now() < expiry) {
        this.accessToken = storedToken;
        this.tokenExpiry = expiry;
        return storedToken;
      }
      // Token expired - return null but keep storage for refresh attempt
      return null;
    }
    return null;
  }

  isAuthenticated(): boolean {
    const user = this.currentUserSubject.value;
    if (useCookieAuth) {
      return !!user;
    }
    const token = this.getAccessToken();
    const hasRefreshToken = !!localStorage.getItem(REFRESH_TOKEN_KEY);
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
    const raw = response.user as User & { Id?: string };
    const user: User = {
      ...raw,
      id: String(raw.id ?? raw.Id ?? ''),
      role: (raw.roleName || raw.role || UserRole.Client) as UserRole
    };

    if (!useCookieAuth) {
      this.accessToken = response.token;
      const expiresAt = new Date(response.expiresAt);
      this.tokenExpiry = expiresAt.getTime();
      localStorage.setItem(TOKEN_KEY, response.token);
      if (response.refreshToken) {
        localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
      }
      localStorage.setItem(EXPIRES_AT_KEY, String(this.tokenExpiry));
      localStorage.setItem(USER_KEY, JSON.stringify(user));
    }

    sessionStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private clearStoredAuth(): void {
    this.accessToken = null;
    this.tokenExpiry = null;
    this.currentUserSubject.next(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
    localStorage.removeItem(USER_KEY);
    sessionStorage.removeItem('user');
  }

  private loadUserFromStorage(): void {
    const userStr = useCookieAuth
      ? sessionStorage.getItem('user')
      : (localStorage.getItem(USER_KEY) || sessionStorage.getItem('user'));

    if (!userStr) {
      this.clearStoredAuth();
      return;
    }

    if (useCookieAuth) {
      try {
        const userData = JSON.parse(userStr);
        const user: User = {
          ...userData,
          id: String(userData.id ?? userData.Id ?? ''),
          role: (userData.role || userData.roleName || UserRole.Client) as UserRole
        };
        this.currentUserSubject.next(user);
      } catch {
        this.clearStoredAuth();
      }
      return;
    }

    const storedToken = localStorage.getItem(TOKEN_KEY);
    const storedExpiry = localStorage.getItem(EXPIRES_AT_KEY);
    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (!storedToken) {
      this.clearStoredAuth();
      return;
    }
    
    const expiry = storedExpiry ? parseInt(storedExpiry, 10) : 0;
    const tokenExpired = Date.now() >= expiry;
    if (tokenExpired && !storedRefreshToken) {
      this.clearStoredAuth();
      return;
    }
    
    try {
      const userData = JSON.parse(userStr);
      const user: User = {
        ...userData,
        id: String(userData.id ?? userData.Id ?? ''),
        role: (userData.role || userData.roleName || UserRole.Client) as UserRole
      };
      if (!tokenExpired) {
        this.accessToken = storedToken;
        this.tokenExpiry = expiry;
      }
      this.currentUserSubject.next(user);
    } catch {
      this.clearStoredAuth();
    }
  }
}
