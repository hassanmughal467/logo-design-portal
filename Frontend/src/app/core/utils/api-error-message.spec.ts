import { HttpErrorResponse } from '@angular/common/http';
import { extractApiErrorMessage, isHttpForbidden } from './api-error-message';

describe('api-error-message', () => {
  it('extractApiErrorMessage reads error field', () => {
    const err = new HttpErrorResponse({
      status: 403,
      error: { error: 'You do not have permission to perform this action (UploadFile).' }
    });
    expect(extractApiErrorMessage(err)).toContain('UploadFile');
  });

  it('extractApiErrorMessage reads message field', () => {
    const err = new HttpErrorResponse({
      status: 403,
      error: { message: 'You do not have access to upload files to this order.' }
    });
    expect(extractApiErrorMessage(err)).toContain('upload files');
  });

  it('extractApiErrorMessage uses fallback for empty forbid body', () => {
    const err = new HttpErrorResponse({ status: 403, error: null });
    expect(extractApiErrorMessage(err, 'No permission')).toBe('No permission');
  });

  it('isHttpForbidden detects 403', () => {
    expect(isHttpForbidden(new HttpErrorResponse({ status: 403 }))).toBeTrue();
    expect(isHttpForbidden(new HttpErrorResponse({ status: 400 }))).toBeFalse();
  });
});
