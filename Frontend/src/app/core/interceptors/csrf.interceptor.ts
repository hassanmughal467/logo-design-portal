import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { environment } from '@environments/environment';
import { AuthService } from '../services/auth.service';

const CSRF_COOKIE = 'ldp_csrf';
const CSRF_HEADER = 'X-XSRF-TOKEN';
const CSRF_SESSION_KEY = 'ldp_csrf';

@Injectable()
export class CsrfInterceptor implements HttpInterceptor {
  constructor(private readonly authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!(environment as { useCookieAuth?: boolean }).useCookieAuth) {
      return next.handle(request);
    }

    if (this.isSafeMethod(request.method)) {
      return next.handle(request);
    }

    const token = sessionStorage.getItem(CSRF_SESSION_KEY) ?? this.readCookie(CSRF_COOKIE);
    if (token) {
      return next.handle(
        request.clone({
          setHeaders: { [CSRF_HEADER]: token }
        })
      );
    }

    // API-host CSRF cookie is not readable from the SPA origin — obtain token before mutating.
    return this.authService.syncCsrfToken().pipe(
      switchMap((synced) => {
        if (!synced) {
          return next.handle(request);
        }
        return next.handle(
          request.clone({
            setHeaders: { [CSRF_HEADER]: synced }
          })
        );
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
