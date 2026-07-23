import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { HttpLoadingErrorInterceptor } from './http-loading-error.interceptor';
import { AuthService } from '../services/auth.service';
import { environment } from '@environments/environment';

describe('HttpLoadingErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  const base = `${environment.apiUrl}/api`;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['logout']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
        { provide: MessageService, useValue: jasmine.createSpyObj('MessageService', ['add']) },
        { provide: HTTP_INTERCEPTORS, useClass: HttpLoadingErrorInterceptor, multi: true }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('does not log out or navigate on a 401 from /auth/refresh-token', () => {
    http.post(`${base}/auth/refresh-token`, {}).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/auth/refresh-token`).flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('logs out and navigates on an ordinary unrecoverable protected-request 401', () => {
    http.get(`${base}/orders`).subscribe({ error: () => {} });
    httpMock.expectOne(`${base}/orders`).flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
