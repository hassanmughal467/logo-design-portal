import type { APIRequestContext } from '@playwright/test';
import { E2E_API_URL } from './env';

// re-export for health probes
export { E2E_API_URL };

/**
 * Thin wrapper around Playwright's `APIRequestContext` with a stable JSON API for this backend.
 * Using Playwright's built-in request fixture gives automatic trace correlation and sane defaults.
 */

export function apiUrl(path: string): string {
  const p = path.startsWith('/') ? path : `/${path}`;
  return `${E2E_API_URL}/api${p}`;
}

export type AuthResponse = {
  token: string;
  refreshToken?: string;
  expiresAt?: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    roleName: string;
  };
};

/** Login via public endpoint; returns JWT for Bearer authorization on subsequent calls. */
export async function loginApi(request: APIRequestContext, email: string, password: string): Promise<AuthResponse> {
  const res = await request.post(apiUrl('/auth/login'), {
    data: { email, password },
    headers: { 'Content-Type': 'application/json' },
  });
  if (!res.ok()) {
    const body = await res.text();
    throw new Error(`loginApi failed: ${res.status()} ${body}`);
  }
  return res.json() as Promise<AuthResponse>;
}

export async function getOrderApi(request: APIRequestContext, token: string, orderId: string) {
  const res = await request.get(apiUrl(`/orders/${orderId}`), {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok()) {
    throw new Error(`getOrderApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string; title: string }>;
}

export async function createOrderApi(
  request: APIRequestContext,
  token: string,
  body: { title: string; description: string; price?: number }
) {
  const res = await request.post(apiUrl('/orders'), {
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    data: { price: 0, ...body },
  });
  if (!res.ok()) {
    throw new Error(`createOrderApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string }>;
}

export async function approveOrderApi(request: APIRequestContext, adminToken: string, orderId: string) {
  const res = await request.post(apiUrl(`/orders/${orderId}/approve`), {
    headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
    data: {},
  });
  if (!res.ok()) {
    throw new Error(`approveOrderApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string }>;
}

export async function assignDesignerApi(
  request: APIRequestContext,
  adminToken: string,
  orderId: string,
  designerUserId: string
) {
  const res = await request.post(apiUrl(`/orders/${orderId}/assign`), {
    headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
    data: { designerId: designerUserId },
  });
  if (!res.ok()) {
    throw new Error(`assignDesignerApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string }>;
}

export async function updateOrderStatusApi(
  request: APIRequestContext,
  token: string,
  orderId: string,
  status: string,
  notes?: string
) {
  const res = await request.put(apiUrl(`/orders/${orderId}/status`), {
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    data: { status, notes: notes ?? '' },
  });
  if (!res.ok()) {
    throw new Error(`updateOrderStatusApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string }>;
}

export async function cancelOrderApi(
  request: APIRequestContext,
  token: string,
  orderId: string,
  reason: string
) {
  const res = await request.post(apiUrl(`/orders/${orderId}/cancel`), {
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    data: { reason },
  });
  if (!res.ok()) {
    throw new Error(`cancelOrderApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json();
}

export async function createUserApi(request: APIRequestContext, adminToken: string, payload: Record<string, unknown>) {
  const res = await request.post(apiUrl('/users'), {
    headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
    data: payload,
  });
  if (!res.ok()) {
    throw new Error(`createUserApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; email: string; roleName?: string }>;
}

export async function requestRevisionApi(
  request: APIRequestContext,
  clientToken: string,
  orderId: string,
  instructions: string
) {
  const res = await request.post(apiUrl(`/revisions/orders/${orderId}/request`), {
    headers: { Authorization: `Bearer ${clientToken}` },
    multipart: { instructions },
  });
  if (!res.ok()) {
    throw new Error(`requestRevisionApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<Record<string, unknown>>;
}

export async function approveLogoApi(
  request: APIRequestContext,
  token: string,
  orderId: string,
  notes = 'Approved via E2E automation.'
) {
  const res = await request.post(apiUrl(`/revisions/orders/${orderId}/approve-logo`), {
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    data: { notes },
  });
  if (!res.ok()) {
    throw new Error(`approveLogoApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string }>;
}

export async function getAdminAnalyticsOverviewApi(
  request: APIRequestContext,
  adminToken: string
): Promise<{ totalOrders: number; pendingOrders: number; totalRevenue: number }> {
  const res = await request.get(apiUrl('/admin/analytics/overview'), {
    headers: { Authorization: `Bearer ${adminToken}` },
  });
  if (!res.ok()) {
    throw new Error(`getAdminAnalyticsOverviewApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ totalOrders: number; pendingOrders: number; totalRevenue: number }>;
}

export async function listInvoicesApi(request: APIRequestContext, token: string) {
  const res = await request.get(apiUrl('/invoices'), {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok()) {
    throw new Error(`listInvoicesApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<Array<{ id: string; status: string; totalAmount?: number }>>;
}

export async function createInvoiceApi(
  request: APIRequestContext,
  adminToken: string,
  body: { orders: Array<{ orderId: string; price: number }>; billingType?: number; taxAmount?: number }
) {
  const res = await request.post(apiUrl('/invoices'), {
    headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
    data: body,
  });
  if (!res.ok()) {
    throw new Error(`createInvoiceApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; invoiceNumber?: string; status?: string }>;
}

export async function markInvoicePaidApi(
  request: APIRequestContext,
  adminToken: string,
  invoiceId: string,
  paymentMethod: string
) {
  const res = await request.put(apiUrl(`/invoices/${invoiceId}/mark-paid`), {
    headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
    data: { paymentMethod },
  });
  if (!res.ok()) {
    throw new Error(`markInvoicePaidApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status?: string }>;
}

export async function getInvoiceApi(request: APIRequestContext, token: string, invoiceId: string) {
  const res = await request.get(apiUrl(`/invoices/${invoiceId}`), {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok()) {
    throw new Error(`getInvoiceApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status?: string; totalAmount?: number }>;
}

export async function refundOrderApi(
  request: APIRequestContext,
  adminToken: string,
  orderId: string,
  amount: number,
  reason: string
) {
  const res = await request.post(apiUrl(`/orders/${orderId}/refund`), {
    headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
    data: { amount, reason },
  });
  if (!res.ok()) {
    throw new Error(`refundOrderApi failed: ${res.status()} ${await res.text()}`);
  }
  return res.json() as Promise<{ id: string; status: string }>;
}

export async function uploadFileApi(
  request: APIRequestContext,
  token: string,
  orderId: string,
  fileName: string,
  body: Buffer,
  contentType: string,
  fileType = 'Reference'
) {
  return request.post(apiUrl(`/files/upload/${orderId}`), {
    headers: { Authorization: `Bearer ${token}` },
    multipart: {
      file: { name: fileName, mimeType: contentType, buffer: body },
      fileType,
    },
  });
}

export async function healthReadyApi(request: APIRequestContext) {
  const base = E2E_API_URL.replace(/\/$/, '');
  return request.get(`${base}/health/ready`);
}

export async function deleteUserApi(request: APIRequestContext, superAdminToken: string, userId: string, permanent = true) {
  const qs = permanent ? '?permanent=true' : '';
  const res = await request.delete(apiUrl(`/users/${userId}${qs}`), {
    headers: { Authorization: `Bearer ${superAdminToken}` },
  });
  if (!res.ok()) {
    throw new Error(`deleteUserApi failed: ${res.status()} ${await res.text()}`);
  }
}
