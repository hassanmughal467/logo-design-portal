import { AppEnvironment } from './environment.interface';

/** CI, Playwright, and local integration UI (align with E2E_API_URL). */
export const environment: AppEnvironment = {
  production: false,
  staging: false,
  apiUrl: 'http://localhost:5000',
  apiVersion: '',
  /** E2E API specs use Bearer tokens; UI tests can enable when cookie auth is fully wired in CI. */
  useCookieAuth: false,
  enableDebugLogging: true
};
