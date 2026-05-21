import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

const CSRF_COOKIE = 'ldp_csrf';
const CSRF_HEADER = 'X-XSRF-TOKEN';

@Injectable()
export class CsrfInterceptor implements HttpInterceptor {
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!(environment as { useCookieAuth?: boolean }).useCookieAuth) {
      return next.handle(request);
    }

    const token = this.readCookie(CSRF_COOKIE);
    if (!token || this.isSafeMethod(request.method)) {
      return next.handle(request);
    }

    return next.handle(
      request.clone({
        setHeaders: { [CSRF_HEADER]: token }
      })
    );
  }

  private isSafeMethod(method: string): boolean {
    const m = method.toUpperCase();
    return m === 'GET' || m === 'HEAD' || m === 'OPTIONS';
  }

  private readCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp(`(?:^|; )${name}=([^;]*)`));
    return match ? decodeURIComponent(match[1]) : null;
  }
}
