import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

export const SKIP_LOADER_HEADER = 'x-skip-loader';

/**
 * Increments global HTTP activity count for observability. Does not block the UI.
 * Strip {@link SKIP_LOADER_HEADER} before the request leaves the browser so it is not sent to the API.
 */
@Injectable()
export class HttpRequestTrackerInterceptor implements HttpInterceptor {
  constructor(private readonly loading: LoadingService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const skipRaw = request.headers.get(SKIP_LOADER_HEADER);
    const skip = skipRaw?.toLowerCase() === 'true';

    let forward = request;
    if (request.headers.has(SKIP_LOADER_HEADER)) {
      forward = request.clone({ headers: request.headers.delete(SKIP_LOADER_HEADER) });
    }

    if (skip) {
      return next.handle(forward);
    }

    this.loading.beginHttp();
    return next.handle(forward).pipe(finalize(() => this.loading.endHttp()));
  }
}
