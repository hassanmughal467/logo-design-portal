import { expect, type APIRequestContext, type TestInfo } from '@playwright/test';
import {
  approveLogoApi,
  approveOrderApi,
  assignDesignerApi,
  createOrderApi,
  getOrderApi,
  requestRevisionApi,
  updateOrderStatusAsAdminApi,
} from './api-client';
import { createTestOrder } from '../factories/test-data.factory';

export type LifecycleActors = {
  adminToken: string;
  clientToken: string;
  designerUserId: string;
  designerToken: string;
};

export async function assertOrderStatusContains(
  request: APIRequestContext,
  token: string,
  orderId: string,
  expectedLower: string
): Promise<void> {
  const snapshot = await getOrderApi(request, token, orderId);
  expect(String(snapshot.status).toLowerCase()).toContain(expectedLower.toLowerCase());
}

/**
 * Canonical business flow used by high-value E2E scenarios.
 */
export async function runCoreBusinessLifecycle(
  request: APIRequestContext,
  testInfo: TestInfo,
  actors: LifecycleActors
): Promise<{ orderId: string }> {
  const order = await createTestOrder(request, actors.clientToken, testInfo, {
    title: `Elite lifecycle ${Date.now()}`,
    description: 'End-to-end lifecycle order from elite suite. Description exceeds minimum length.',
    price: 250,
  });

  await assertOrderStatusContains(request, actors.clientToken, order.id, String(order.status));
  // Approve-only parks the order at ApprovedUnassigned; assigning the designer moves it to InProgress.
  await approveOrderApi(request, actors.adminToken, order.id);
  await assignDesignerApi(request, actors.adminToken, order.id, actors.designerUserId);
  await assertOrderStatusContains(request, actors.clientToken, order.id, 'progress');

  // Designers no longer set PreviewDelivered via the status endpoint — an admin forwards files
  // to the client (SendFilesToClient). The admin status transition models that forwarding step.
  await updateOrderStatusAsAdminApi(request, actors.adminToken, order.id, 'PreviewDelivered', 'Preview forwarded to client.');
  await assertOrderStatusContains(request, actors.clientToken, order.id, 'preview');

  await requestRevisionApi(request, actors.clientToken, order.id, 'Please tighten spacing and improve icon balance.');
  await updateOrderStatusAsAdminApi(request, actors.adminToken, order.id, 'PreviewDelivered', 'Revision forwarded to client.');
  await approveLogoApi(request, actors.clientToken, order.id, 'Looks good after revision.');
  await updateOrderStatusAsAdminApi(request, actors.adminToken, order.id, 'Completed', 'Closed by automation suite.');
  await assertOrderStatusContains(request, actors.clientToken, order.id, 'completed');

  return { orderId: order.id };
}
