import { AppEnvironment } from './environment.interface';

export const environment: AppEnvironment = {
  production: false,
  staging: true,
  apiUrl: 'https://staging-api.hawkmerchandising.com',
  apiVersion: '',
  useCookieAuth: true,
  enableDebugLogging: false
};
