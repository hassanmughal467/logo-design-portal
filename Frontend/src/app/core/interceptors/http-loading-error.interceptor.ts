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
import { extractApiErrorMessage } from '../utils/api-error-message';

const HTTP_TIMEOUT_MS = 15000;
const RATE_LIMIT_TOAST_COOLDOWN_MS = 10_000;

/** Handles timeouts and HTTP errors (toasts, auth). Does not show any blocking or full-screen loader. */
@Injectable()
export class HttpLoadingErrorInterceptor implements HttpInterceptor {
  private static lastRateLimitToastAt = 0;

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

    const message = extractApiErrorMessage(error);

    const correlationId = error?.error?.correlationId;
    if (correlationId) {
      console.error(`[HTTP ${error.status}] correlationId=${correlationId}`, {
        url: request.url,
        message
      });
    }

    const requestUrl = (request.url || '').toLowerCase();
    const isAuthEndpoint = requestUrl.includes('/auth/login') || requestUrl.includes('/auth/register');

    switch (error.status) {
      case 401:
        // TokenInterceptor owns refresh + logout; avoid racing double-logout here.
        if (!isAuthEndpoint && !request.headers.has('X-Auth-Handled')) {
          // No action — session messaging handled by TokenInterceptor.
        }
        break;
      case 403: {
        // Upload components show their own permission-aware toast (avoids duplicate generic messages).
        const isFileUploadEndpoint =
          requestUrl.includes('/files/upload') || requestUrl.includes('/files/upload-multiple');
        if (!isFileUploadEndpoint) {
          this.messageService.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: message || 'You do not have permission to perform this action.'
          });
        }
        break;
      }
      case 429:
        if (Date.now() - HttpLoadingErrorInterceptor.lastRateLimitToastAt >= RATE_LIMIT_TOAST_COOLDOWN_MS) {
          HttpLoadingErrorInterceptor.lastRateLimitToastAt = Date.now();
          this.messageService.add({
            severity: 'warn',
            summary: 'Too Many Requests',
            detail: message || 'Too many requests, please try again later'
          });
        }
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
