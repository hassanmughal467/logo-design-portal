import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { of, Subject } from 'rxjs';
import { TokenInterceptor } from './token.interceptor';
import { HttpLoadingErrorInterceptor } from './http-loading-error.interceptor';
import { AuthService } from '../services/auth.service';
import { environment } from '@environments/environment';

/**
 * Regression coverage for fix/silent-refresh-interceptor-order.
 *
 * Registers HttpLoadingErrorInterceptor and TokenInterceptor in the same relative
 * order as app.module.ts (TokenInterceptor last, so it sees response errors first).
 * Reproduces a reload-like startup: no in-memory access token, but refresh tokens
 * are still present (as they would be in sessionStorage after a reload).
 */
describe('Interceptor order: silent refresh vs. eager logout', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  const base = `${environment.apiUrl}/api`;

  beforeEach(() => {
    // Reload-like startup: in-memory access token is gone, but the refresh-token
    // keys written to sessionStorage before the reload are still present.
    authService = jasmine.createSpyObj('AuthService', [
      'getAccessToken',
      'getStoredTokensForRefresh',
      'refreshToken',
      'logout'
    ]);
    authService.getAccessToken.and.returnValues(null, 'fresh-token');
    authService.getStoredTokensForRefresh.and.returnValue({ token: 'stale-jwt', refreshToken: 'r1' });
    authService.refreshToken.and.returnValue(of({} as any));

    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
        { provide: MessageService, useValue: jasmine.createSpyObj('MessageService', ['add']) },
        // Same relative order as app.module.ts: TokenInterceptor registered last
        // so it is the first to see a response error.
        { provide: HTTP_INTERCEPTORS, useClass: HttpLoadingErrorInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('attempts a silent refresh and retries the request instead of logging out on reload', () => {
    let result: unknown;
    http.get(`${base}/orders/my-orders`).subscribe({
      next: (res) => (result = res),
      error: (err) => (result = err)
    });

    // 1. The protected request comes back 401 after reload-like startup.
    const first = httpMock.expectOne(`${base}/orders/my-orders`);
    first.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' } as HttpErrorResponse & { status: number });

    // 2. Stored refresh tokens were present, so refresh must be attempted...
    expect(authService.getStoredTokensForRefresh).toHaveBeenCalled();
    expect(authService.refreshToken).toHaveBeenCalled();

    // 3. ...and logout must NOT be called before (or instead of) that refresh.
    expect(authService.logout).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();

    // 4. The original protected request is retried once refresh succeeds.
    const retry = httpMock.expectOne(`${base}/orders/my-orders`);
    expect(retry.request.headers.get('Authorization')).toBe('Bearer fresh-token');
    retry.flush([{ id: 1 }]);

    expect(result).toEqual([{ id: 1 }]);
    expect(authService.logout).not.toHaveBeenCalled();
  });

  it('shares a single refresh across concurrent reload-time 401s and does not log out on recovery', () => {
    authService.getAccessToken.and.returnValues(null, null, 'fresh-token', 'fresh-token');
    const refresh$ = new Subject<any>();
    authService.refreshToken.and.returnValue(refresh$.asObservable());

    let result1: unknown;
    let result2: unknown;
    http.get(`${base}/orders/my-orders`).subscribe({ next: (r) => (result1 = r), error: (e) => (result1 = e) });
    http.get(`${base}/notifications`).subscribe({ next: (r) => (result2 = r), error: (e) => (result2 = e) });

    httpMock.expectOne(`${base}/orders/my-orders`).flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' } as HttpErrorResponse & { status: number });
    httpMock.expectOne(`${base}/notifications`).flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' } as HttpErrorResponse & { status: number });

    // Two concurrent reload-time 401s must queue behind one shared refresh, not start two.
    expect(authService.refreshToken).toHaveBeenCalledTimes(1);
    expect(authService.logout).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();

    refresh$.next({});
    refresh$.complete();

    const retry1 = httpMock.expectOne(`${base}/orders/my-orders`);
    const retry2 = httpMock.expectOne(`${base}/notifications`);
    expect(retry1.request.headers.get('Authorization')).toBe('Bearer fresh-token');
    expect(retry2.request.headers.get('Authorization')).toBe('Bearer fresh-token');
    retry1.flush([{ id: 1 }]);
    retry2.flush([{ id: 2 }]);

    expect(result1).toEqual([{ id: 1 }]);
    expect(result2).toEqual([{ id: 2 }]);
    expect(authService.logout).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
