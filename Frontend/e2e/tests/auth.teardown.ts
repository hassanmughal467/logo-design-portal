import { test as teardown } from '@playwright/test';
import path from 'path';
import fs from 'fs';
import { teardownUsers } from '../utils/api-helpers';

/**
 * Runs once, after every project that depends on `setup` (see `dependencies: ['setup']` /
 * `teardown: 'cleanup-admin'` in playwright.config.ts) has finished. Deletes the dedicated admin
 * account `auth.setup.ts` provisioned for the shared `admin.json` storageState session.
 */
const adminMetaFile = path.join(__dirname, '..', '.auth', 'admin-meta.json');

teardown('remove dedicated admin operator', async ({ request }) => {
  if (!fs.existsSync(adminMetaFile)) {
    return;
  }
  const { userId } = JSON.parse(fs.readFileSync(adminMetaFile, 'utf-8')) as { userId: string };
  await teardownUsers(request, [userId]);
  fs.rmSync(adminMetaFile, { force: true });
});
