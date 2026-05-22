import { test, expect } from '@playwright/test';

import { apiUrl, loginApi } from '../../utils/api-client';

import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

import { getAdminToken, provisionLoggedInClient, teardownUsers } from '../../utils/api-helpers';



test.describe('Workflow — invoice and payment permissions API', () => {

  const cleanup: string[] = [];



  test.afterAll(async ({ request }) => {

    await teardownUsers(request, cleanup);

  });



  test('client cannot create invoice', async ({ request }, testInfo) => {

    const client = await provisionLoggedInClient(request, testInfo);

    cleanup.push(client.userId);



    const response = await request.post(apiUrl('/invoices'), {

      headers: {

        Authorization: `Bearer ${client.token}`,

        'Content-Type': 'application/json',

      },

      data: {

        orders: [],

        billingType: 'PerLogo',

        taxAmount: 0,

      },

    });

    expect(response.status()).toBe(403);

  });



  test('admin can list invoices', async ({ request }) => {

    const adminAuth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);

    const response = await request.get(apiUrl('/invoices'), {

      headers: { Authorization: `Bearer ${adminAuth.token}` },

    });

    expect(response.ok()).toBeTruthy();

  });



  test('client can read bank details', async ({ request }, testInfo) => {

    const client = await provisionLoggedInClient(request, testInfo);

    cleanup.push(client.userId);



    const response = await request.get(apiUrl('/payments/bank-details'), {

      headers: { Authorization: `Bearer ${client.token}` },

    });

    expect(response.ok()).toBeTruthy();

  });

});


