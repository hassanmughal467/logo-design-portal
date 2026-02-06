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
              // Unauthorized - token expired or invalid
              this.authService.logout();
              this.messageService.add({
                severity: 'error',
                summary: 'Session Expired',
                detail: 'Please login again'
              });
              break;

            case 403:
              // Forbidden - user doesn't have permission
              errorMessage = 'You do not have permission to perform this action';
              this.messageService.add({
                severity: 'error',
                summary: 'Access Denied',
                detail: errorMessage
              });
              break;

            case 404:
              errorMessage = 'Resource not found';
              break;

            case 500:
              errorMessage = 'Server error. Please try again later';
              this.messageService.add({
                severity: 'error',
                summary: 'Server Error',
                detail: errorMessage
              });
              break;

            default:
              errorMessage = error.error?.message || `Error Code: ${error.status}`;
              if (error.status >= 400 && error.status < 500) {
                this.messageService.add({
                  severity: 'warn',
                  summary: 'Request Error',
                  detail: errorMessage
                });
              }
          }
        }

        return throwError(() => error);
      })
    );
  }
}
