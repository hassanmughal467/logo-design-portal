import type { APIRequestContext } from '@playwright/test';
import { apiUrl, loginApi, type AuthResponse } from '../utils/api-client';

export async function loginAs(
  request: APIRequestContext,
  email: string,
  password: string
): Promise<AuthResponse> {
  return loginApi(request, email, password);
}

export async function forgotPasswordApi(request: APIRequestContext, email: string) {
  return request.post(apiUrl('/auth/forgot-password'), {
    data: { email },
    headers: { 'Content-Type': 'application/json' },
  });
}

export async function refreshTokenApi(request: APIRequestContext, refreshToken: string) {
  return request.post(apiUrl('/auth/refresh-token'), {
    data: { refreshToken },
    headers: { 'Content-Type': 'application/json' },
  });
}

export async function protectedProbe(request: APIRequestContext, token?: string) {
  return request.get(apiUrl('/orders/my-orders'), {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
}
