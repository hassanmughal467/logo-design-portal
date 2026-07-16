import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ApiService } from './api.service';
import { DashboardService } from './dashboard.service';
import { SharedListDataService } from './shared-list-data.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let router: jasmine.SpyObj<Router>;
  let dashboardService: jasmine.SpyObj<DashboardService>;
  let sharedListDataService: jasmine.SpyObj<SharedListDataService>;

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigate']);
    dashboardService = jasmine.createSpyObj('DashboardService', ['invalidateDashboardCache']);
    sharedListDataService = jasmine.createSpyObj('SharedListDataService', ['clearAll']);
    localStorage.clear();
    sessionStorage.clear();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        ApiService,
        { provide: Router, useValue: router },
        { provide: DashboardService, useValue: dashboardService },
        { provide: SharedListDataService, useValue: sharedListDataService },
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return false for isAuthenticated when no user', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  it('should return null for getCurrentUser when not logged in', () => {
    expect(service.getCurrentUser()).toBeNull();
  });

  it('should return false for hasRole when no user', () => {
    expect(service.hasRole('Admin')).toBe(false);
  });

  it('should return false for hasAnyRole when no user', () => {
    expect(service.hasAnyRole(['Admin', 'Client'])).toBe(false);
  });

  it('login persists user and token', (done) => {
    const future = new Date(Date.now() + 60_000).toISOString();
    service.login({ email: 'a@b.com', password: 'x' }).subscribe({
      next: (res) => {
        expect(res.token).toBe('tok');
        expect(service.getCurrentUser()?.email).toBe('a@b.com');
        expect(service.getAccessToken()).toBe('tok');
        expect(service.hasRole('Admin')).toBe(true);
        expect(service.hasAnyRole(['Client', 'Admin'])).toBe(true);
        expect(service.isAuthenticated()).toBe(true);
        done();
      },
    });
    const req = httpMock.expectOne((r) => r.url.includes('/auth/login'));
    expect(req.request.method).toBe('POST');
    req.flush({
      token: 'tok',
      refreshToken: 'ref',
      expiresAt: future,
      user: { id: '1', email: 'a@b.com', firstName: 'A', lastName: 'B', roleName: 'Admin' },
    });
  });

  it('logout clears storage and navigates', () => {
    localStorage.setItem('auth_token', 't');
    localStorage.setItem('auth_refresh_token', 'r');
    localStorage.setItem('auth_expires_at', String(Date.now() + 99999));
    localStorage.setItem('auth_user', JSON.stringify({ email: 'x@y.com', roleName: 'Client' }));
    service['currentUserSubject'].next({ email: 'x@y.com', roleName: 'Client' } as any);
    service.logout();
    expect(localStorage.getItem('auth_token')).toBeNull();
    expect(sharedListDataService.clearAll).toHaveBeenCalled();
    expect(dashboardService.invalidateDashboardCache).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('getStoredTokensForRefresh returns pair when present', () => {
    localStorage.setItem('auth_token', 'a');
    localStorage.setItem('auth_refresh_token', 'b');
    expect(service.getStoredTokensForRefresh()).toEqual({ token: 'a', refreshToken: 'b' });
  });

  it('getAccessToken returns null when stored expiry elapsed', () => {
    localStorage.setItem('auth_token', 'old');
    localStorage.setItem('auth_expires_at', String(Date.now() - 1000));
    expect(service.getAccessToken()).toBeNull();
  });

  it('refreshToken posts and updates session', (done) => {
    localStorage.setItem('auth_token', 'old');
    localStorage.setItem('auth_refresh_token', 'r');
    const future = new Date(Date.now() + 120_000).toISOString();
    service.refreshToken().subscribe({
      next: () => {
        expect(service.getAccessToken()).toBe('newtok');
        done();
      },
    });
    const req = httpMock.expectOne((r) => r.url.includes('/auth/refresh-token'));
    req.flush({
      token: 'newtok',
      refreshToken: 'r2',
      expiresAt: future,
      user: { id: '1', email: 'a@b.com', firstName: 'A', lastName: 'B', roleName: 'Client' },
    });
  });

  it('refreshToken without stored tokens errors', (done) => {
    localStorage.clear();
    service.refreshToken().subscribe({
      error: (e) => {
        expect(e).toBeTruthy();
        done();
      },
    });
  });

  it('register, changePassword, forgotPassword, reset helpers forward to API', () => {
    service.register({} as any).subscribe();
    httpMock.expectOne((r) => r.url.includes('/auth/register')).flush({});

    service.changePassword({ currentPassword: 'a', newPassword: 'b', confirmPassword: 'b' }).subscribe();
    httpMock.expectOne((r) => r.url.includes('/auth/change-password')).flush({});

    service.forgotPassword('e@e.com').subscribe();
    httpMock.expectOne((r) => r.url.includes('/auth/forgot-password')).flush({});

    service.resetPasswordWithToken({ email: 'e', token: 't', newPassword: 'n', confirmPassword: 'n' }).subscribe();
    httpMock.expectOne((r) => r.url.includes('/auth/reset-password-with-token')).flush({});

    service.resetUserPassword('uid', 'pw').subscribe();
    httpMock.expectOne((r) => r.url.includes('/auth/reset-password')).flush({});
  });
});
