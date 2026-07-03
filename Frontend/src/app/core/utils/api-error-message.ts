import { HttpErrorResponse } from '@angular/common/http';

/** Reads `{ error }` or `{ message }` from API error bodies (and plain-text bodies). */
export function extractApiErrorMessage(error: unknown, fallback = 'Something went wrong'): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const body = error.error;
  if (typeof body === 'string' && body.trim()) {
    return body.trim();
  }

  if (body && typeof body === 'object') {
    const record = body as { error?: unknown; message?: unknown };
    if (typeof record.error === 'string' && record.error.trim()) {
      return record.error.trim();
    }
    if (typeof record.message === 'string' && record.message.trim()) {
      return record.message.trim();
    }
  }

  return fallback;
}

export function isHttpForbidden(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 403;
}
