import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { MessageService } from 'primeng/api';
import { environment } from '@environments/environment';

const useCookieAuth = !!(environment as { useCookieAuth?: boolean }).useCookieAuth;

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private messageService: MessageService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = useCookieAuth ? null : this.authService.getAccessToken();
    const authRequest = token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

    return next.handle(authRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        const isAuthEndpoint = (request.url || '').includes('/auth/login') || (request.url || '').includes('/auth/register');
        const isRefreshEndpoint = (request.url || '').includes('/auth/refresh-token');

        if (error.status === 401 && !isAuthEndpoint && !isRefreshEndpoint) {
          const canRefresh = useCookieAuth || this.authService.getStoredTokensForRefresh();
          if (canRefresh) {
            return this.authService.refreshToken().pipe(
              switchMap(() => {
                const newToken = this.authService.getAccessToken();
                const retryRequest = request.clone({
                  setHeaders: { Authorization: `Bearer ${newToken}` }
                });
                return next.handle(retryRequest);
              }),
              catchError(() => throwError(() => error))
            );
          }
          this.authService.logout();
          this.messageService.add({
            severity: 'error',
            summary: 'Session Expired',
            detail: 'Please login again'
          });
        }
        return throwError(() => error);
      })
    );
  }
}
