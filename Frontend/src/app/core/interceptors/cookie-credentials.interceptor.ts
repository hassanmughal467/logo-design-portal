import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

/** Sends cookies on API requests when cookie-based auth is enabled. */
@Injectable()
export class CookieCredentialsInterceptor implements HttpInterceptor {
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!(environment as { useCookieAuth?: boolean }).useCookieAuth) {
      return next.handle(request);
    }

    return next.handle(request.clone({ withCredentials: true }));
  }
}
