import { AppEnvironment } from './environment.interface';

export const environment: AppEnvironment = {
  production: false,
  staging: false,
  apiUrl: 'https://localhost:44398',
  apiVersion: '',
  useCookieAuth: true,
  enableDebugLogging: true
};
