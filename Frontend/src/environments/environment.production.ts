import { AppEnvironment } from './environment.interface';

export const environment: AppEnvironment = {
  production: true,
  staging: false,
  apiUrl: 'https://api.hawkmerchandising.com',
  apiVersion: '',
  useCookieAuth: true,
  enableDebugLogging: false
};
