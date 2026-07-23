import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Observable, throwError, TimeoutError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { AuthService } from '../services/auth.service';

const HTTP_TIMEOUT_MS = 15000;

/** Handles timeouts and HTTP errors (toasts, auth). Does not show any blocking or full-screen loader. */
@Injectable()
export class HttpLoadingErrorInterceptor implements HttpInterceptor {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly messageService: MessageService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      timeout(HTTP_TIMEOUT_MS),
      catchError((error: unknown) => {
        this.handleError(request, error);
        return throwError(() => error);
      })
    );
  }

  private handleError(request: HttpRequest<unknown>, error: unknown): void {
    if (error instanceof TimeoutError) {
      this.messageService.add({
        severity: 'error',
        summary: 'Request Timeout',
        detail: 'Request timed out. Please try again.'
      });
      return;
    }

    if (!(error instanceof HttpErrorResponse)) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Something went wrong'
      });
      return;
    }

    const message =
      error?.error?.message ||
      error?.error?.error ||
      'Something went wrong';

    const correlationId = error?.error?.correlationId;
    if (correlationId) {
      console.error(`[HTTP ${error.status}] correlationId=${correlationId}`, {
        url: request.url,
        message
      });
    }

    const requestUrl = (request.url || '').toLowerCase();
    const isAuthEndpoint = requestUrl.includes('/auth/login') || requestUrl.includes('/auth/register');
    // AuthService.refreshToken() already owns logout on a failed refresh; don't duplicate it here.
    const isRefreshEndpoint = requestUrl.includes('/auth/refresh-token');

    switch (error.status) {
      case 401:
        if (!isAuthEndpoint && !isRefreshEndpoint) {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
        break;
      case 403:
        this.messageService.add({
          severity: 'error',
          summary: 'Access Denied',
          detail: 'Access denied'
        });
        break;
      case 429:
        this.messageService.add({
          severity: 'warn',
          summary: 'Too Many Requests',
          detail: 'Too many requests, please try again later'
        });
        break;
      case 500:
        this.messageService.add({
          severity: 'error',
          summary: 'Server Error',
          detail: message
        });
        break;
      default:
        this.messageService.add({
          severity: error.status >= 500 ? 'error' : 'warn',
          summary: error.status >= 500 ? 'Server Error' : 'Request Error',
          detail: message
        });
        break;
    }
  }
}
