import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TokenInterceptor } from './token.interceptor';
import { AuthService } from '../services/auth.service';
import { MessageService } from 'primeng/api';
import { environment } from '@environments/environment';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';

describe('TokenInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;
  let messageService: jasmine.SpyObj<MessageService>;
  const base = `${environment.apiUrl}/api`;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', [
      'getAccessToken',
      'getStoredTokensForRefresh',
      'refreshToken',
      'logout'
    ]);
    messageService = jasmine.createSpyObj<MessageService>('MessageService', ['add']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: MessageService, useValue: messageService },
        { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('adds Authorization header when token exists', (done) => {
    authService.getAccessToken.and.returnValue('token-123');
    http.get(`${base}/orders`).subscribe(() => done());
    const req = httpMock.expectOne(`${base}/orders`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-123');
    req.flush([]);
  });

  it('does not add header when token missing', () => {
    authService.getAccessToken.and.returnValue(null);
    http.get(`${base}/orders`).subscribe();
    const req = httpMock.expectOne(`${base}/orders`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush([]);
  });

  it('does not retry refresh on auth/login 401', () => {
    authService.getAccessToken.and.returnValue('bad');
    http.post(`${base}/auth/login`, {}).subscribe({
      error: () => {
        expect(authService.refreshToken).not.toHaveBeenCalled();
      }
    });
    httpMock.expectOne(`${base}/auth/login`).flush(null, {
      status: 401,
      statusText: 'Unauthorized'
    });
  });

  it('propagates errors for auth endpoints without logout side-effects on register 401', () => {
    authService.getAccessToken.and.returnValue('t');
    http.post(`${base}/auth/register`, {}).subscribe({
      error: (e: HttpErrorResponse) => expect(e.status).toBe(401)
    });
    httpMock.expectOne(`${base}/auth/register`).flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(authService.logout).not.toHaveBeenCalled();
  });

  it('tries refresh and retries original request on 401', () => {
    authService.getAccessToken.and.returnValues('expired-token', 'fresh-token');
    authService.getStoredTokensForRefresh.and.returnValue({ token: 'expired-token', refreshToken: 'r1' });
    authService.refreshToken.and.returnValue(of({} as any));

    http.get(`${base}/orders/my-orders`).subscribe();

    const first = httpMock.expectOne(`${base}/orders/my-orders`);
    expect(first.request.headers.get('Authorization')).toBe('Bearer expired-token');
    first.flush({}, { status: 401, statusText: 'Unauthorized' });

    const retry = httpMock.expectOne(`${base}/orders/my-orders`);
    expect(retry.request.headers.get('Authorization')).toBe('Bearer fresh-token');
    retry.flush([]);

    expect(authService.refreshToken).toHaveBeenCalled();
    expect(authService.logout).not.toHaveBeenCalled();
  });

  it('logs out when 401 and no refresh tokens exist', () => {
    authService.getAccessToken.and.returnValue('expired-token');
    authService.getStoredTokensForRefresh.and.returnValue(null);

    http.get(`${base}/orders`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/orders`).flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).toHaveBeenCalled();
    expect(messageService.add).toHaveBeenCalledWith(
      jasmine.objectContaining({ summary: 'Session Expired' })
    );
  });

  it('returns original 401 when refresh call fails', () => {
    authService.getAccessToken.and.returnValue('expired-token');
    authService.getStoredTokensForRefresh.and.returnValue({ token: 'expired-token', refreshToken: 'r1' });
    authService.refreshToken.and.returnValue(throwError(() => new Error('refresh failed')));

    http.get(`${base}/orders`).subscribe({
      error: (e: HttpErrorResponse) => {
        expect(e.status).toBe(401);
      }
    });

    httpMock.expectOne(`${base}/orders`).flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(authService.refreshToken).toHaveBeenCalled();
  });
});
