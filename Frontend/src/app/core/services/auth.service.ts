import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '@shared/models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  // Store access token in memory (more secure than localStorage)
  private accessToken: string | null = null;
  private tokenExpiry: number | null = null;

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {
    // Check if user is already logged in (on app refresh)
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
    return this.apiService.post<LoginResponse>('auth/refresh-token', {}).pipe(
      tap(response => {
        this.setAuthData(response);
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.accessToken = null;
    this.tokenExpiry = null;
    this.currentUserSubject.next(null);
    // Clear any stored data
    sessionStorage.removeItem('user');
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    // Check if token is expired
    if (this.tokenExpiry && Date.now() >= this.tokenExpiry) {
      // Token expired, try to refresh
      this.refreshToken().subscribe({
        error: () => this.logout()
      });
      return null;
    }
    return this.accessToken;
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken() && !!this.currentUserSubject.value;
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
    this.accessToken = response.token;  // Backend returns 'token' not 'accessToken'
    // Parse expiresAt DateTime string from backend
    const expiresAt = new Date(response.expiresAt);
    this.tokenExpiry = expiresAt.getTime();
    
    // Map backend user object to frontend User model
    // Backend returns 'roleName' but frontend expects 'role'
    const user: User = {
      ...response.user,
      role: (response.user as any).roleName || (response.user as any).role || 'Client'
    };
    
    // Store user in sessionStorage for persistence across page refreshes
    sessionStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private loadUserFromStorage(): void {
    const userStr = sessionStorage.getItem('user');
    if (userStr) {
      try {
        const userData = JSON.parse(userStr);
        // Map roleName to role if needed (for backward compatibility)
        const user: User = {
          ...userData,
          role: userData.role || userData.roleName || 'Client'
        };
        this.currentUserSubject.next(user);
        // Note: Token should be refreshed on app load for security
      } catch (error) {
        sessionStorage.removeItem('user');
      }
    }
  }
}
