import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { ErrorInterceptor } from './error.interceptor';
import { AuthService } from '../services/auth.service';
import { MessageService } from 'primeng/api';
import { environment } from '@environments/environment';

describe('ErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;
  let messageService: jasmine.SpyObj<MessageService>;
  let router: jasmine.SpyObj<Router>;
  const base = `${environment.apiUrl}/api`;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['logout']);
    messageService = jasmine.createSpyObj<MessageService>('MessageService', ['add']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: MessageService, useValue: messageService },
        { provide: Router, useValue: router },
        { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('passes success responses through', (done) => {
    http.get(`${base}/x`).subscribe((body) => {
      expect(body).toEqual({ ok: true });
      done();
    });
    httpMock.expectOne(`${base}/x`).flush({ ok: true });
  });

  it('on 403 for /orders does not toast (graceful handling)', () => {
    http.get(`${base}/orders`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/orders`).flush({ error: 'no' }, { status: 403, statusText: 'Forbidden' });
    expect(messageService.add).not.toHaveBeenCalled();
  });

  it('on 403 for unexpected path shows toast', () => {
    http.get(`${base}/something-else`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/something-else`).flush({ error: 'x' }, { status: 403, statusText: 'Forbidden' });
    expect(messageService.add).toHaveBeenCalled();
  });

  it('on 500 shows server error toast', () => {
    http.get(`${base}/boom`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/boom`).flush({ error: 'fail' }, { status: 500, statusText: 'Error' });
    expect(messageService.add).toHaveBeenCalledWith(
      jasmine.objectContaining({ severity: 'error', summary: 'Server Error' })
    );
  });

  it('401 on refresh-token path logs out', () => {
    http.post(`${base}/auth/refresh-token`, {}).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/auth/refresh-token`).flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(authService.logout).toHaveBeenCalled();
  });

  it('401 on auth/login does not force logout from interceptor', () => {
    http.post(`${base}/auth/login`, {}).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/auth/login`).flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(authService.logout).not.toHaveBeenCalled();
  });

  it('client-side ErrorEvent maps message', (done) => {
    http.get(`${base}/c`).subscribe({
      error: () => done(),
    });
    const req = httpMock.expectOne(`${base}/c`);
    req.error(new ErrorEvent('fail', { message: 'offline' }));
  });

  it('404 maps to resource not found string path', (done) => {
    http.get(`${base}/missing`).subscribe({
      error: (err) => {
        expect(err.status).toBe(404);
        done();
      },
    });
    httpMock.expectOne(`${base}/missing`).flush(null, { status: 404, statusText: 'Not Found' });
  });

  it('default 4xx shows warn toast except file upload endpoints', () => {
    http.get(`${base}/widgets`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/widgets`).flush({ message: 'bad' }, { status: 400, statusText: 'Bad' });
    expect(messageService.add).toHaveBeenCalledWith(
      jasmine.objectContaining({ severity: 'warn', summary: 'Request Error' })
    );
  });

  it('default 4xx skips toast for file upload urls', () => {
    messageService.add.calls.reset();
    http.post(`${base}/files/upload`, {}).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/files/upload`).flush({}, { status: 400, statusText: 'Bad' });
    expect(messageService.add).not.toHaveBeenCalled();
  });

  it('500 appends detail when present', () => {
    http.get(`${base}/s`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/s`).flush(
      { error: 'x', detail: 'more' },
      { status: 500, statusText: 'Err' }
    );
    const arg = messageService.add.calls.mostRecent().args[0] as { detail: string };
    expect(arg.detail).toContain('more');
  });

  it('403 for /permissions is suppressed', () => {
    messageService.add.calls.reset();
    http.get(`${base}/permissions`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/permissions`).flush({ error: 'n' }, { status: 403, statusText: 'Forbidden' });
    expect(messageService.add).not.toHaveBeenCalled();
  });
});
