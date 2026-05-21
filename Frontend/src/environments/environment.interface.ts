/**
 * Build-time environment contract. Values are replaced per Angular configuration
 * (development, testing/e2e, staging, production).
 */
export interface AppEnvironment {
  production: boolean;
  staging: boolean;
  apiUrl: string;
  apiVersion: string;
  /** When set, overrides apiUrl for SignalR hub connections only. */
  signalRUrl?: string;
  useCookieAuth: boolean;
  enableDebugLogging: boolean;
}
