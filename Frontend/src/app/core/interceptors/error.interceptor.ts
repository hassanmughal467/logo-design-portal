import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { MessageService } from 'primeng/api';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unknown error occurred';

        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Server-side error
          switch (error.status) {
            case 401:
              // Unauthorized - TokenInterceptor tries refresh first; ErrorInterceptor only handles when refresh fails (401 on refresh-token)
              const isAuthEndpoint = (request.url || '').includes('/auth/login') || (request.url || '').includes('/auth/register');
              const isRefreshEndpoint = (request.url || '').includes('/auth/refresh-token');
              if (!isAuthEndpoint && isRefreshEndpoint) {
                this.authService.logout();
                this.messageService.add({
                  severity: 'error',
                  summary: 'Session Expired',
                  detail: 'Please login again'
                });
              }
              break;

            case 403:
              // Forbidden - use backend message when available
              errorMessage = error.error?.error || 'You do not have permission to perform this action';
              // Don't show alert for certain endpoints where 403 is expected for some roles
              // These services handle the errors gracefully
              const requestUrl = (request.url || '').toLowerCase();
              const errorUrl = (error.url || '').toLowerCase();
              
              // Endpoints where 403 errors are expected and handled gracefully:
              // - /permissions: Only SuperAdmin has access
              // - /orders: Admin users may not have ViewAllOrders permission yet
              // - /invoices: May not be accessible to all roles
              // - /users: Only Admin/SuperAdmin have access (clients may try to access this)
              const isExpected403Endpoint = 
                requestUrl.includes('/permissions') || errorUrl.includes('/permissions') ||
                requestUrl.includes('/orders') || errorUrl.includes('/orders') ||
                requestUrl.includes('/invoices') || errorUrl.includes('/invoices') ||
                requestUrl.includes('/users') || errorUrl.includes('/users');
              
              if (!isExpected403Endpoint) {
                this.messageService.add({
                  severity: 'error',
                  summary: 'Access Denied',
                  detail: errorMessage
                });
              }
              // Expected 403 on certain endpoints - services handle gracefully, no toast needed
              break;

            case 404:
              errorMessage = 'Resource not found';
              break;

            case 500:
              errorMessage = error.error?.error || 'Server error. Please try again later';
              if (error.error?.detail) {
                errorMessage += ` (${error.error.detail})`;
              }
              this.messageService.add({
                severity: 'error',
                summary: 'Server Error',
                detail: errorMessage
              });
              break;

            default:
              // Check for error message in common locations
              errorMessage = error.error?.error || error.error?.message || `Error Code: ${error.status}`;
              if (error.status >= 400 && error.status < 500) {
                // Skip global toast for endpoints that handle errors in-component (avoids duplicate messages)
                const url = (request.url || '').toLowerCase();
                const isFileUploadEndpoint = url.includes('/files/upload') || url.includes('/files/upload-multiple');
                if (!isFileUploadEndpoint) {
                  this.messageService.add({
                    severity: 'warn',
                    summary: 'Request Error',
                    detail: errorMessage
                  });
                }
              }
          }
        }

        return throwError(() => error);
      })
    );
  }
}
