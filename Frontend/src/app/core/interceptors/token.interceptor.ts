import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, shareReplay, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { LoginResponse } from '@shared/models/user.model';
import { MessageService } from 'primeng/api';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  // Memoized in-flight refresh so concurrent 401s share one call instead of racing rotated refresh tokens.
  private refreshInFlight$: Observable<LoginResponse> | null = null;

  constructor(
    private authService: AuthService,
    private messageService: MessageService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getAccessToken();
    const authRequest = token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

    return next.handle(authRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        const isAuthEndpoint = (request.url || '').includes('/auth/login') || (request.url || '').includes('/auth/register');
        const isRefreshEndpoint = (request.url || '').includes('/auth/refresh-token');

        if (error.status === 401 && !isAuthEndpoint && !isRefreshEndpoint) {
          const stored = this.authService.getStoredTokensForRefresh();
          if (stored) {
            return this.sharedRefresh().pipe(
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

  // Starts (or reuses) the single in-flight refresh; finalize resets the memo on success or failure.
  private sharedRefresh(): Observable<LoginResponse> {
    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.authService.refreshToken().pipe(
        finalize(() => {
          this.refreshInFlight$ = null;
        }),
        shareReplay(1)
      );
    }
    return this.refreshInFlight$;
  }
}
