import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = __ENV.API_BASE_URL || 'http://localhost:5000';
const ADMIN_TOKEN = __ENV.K6_ADMIN_TOKEN || '';

export const options = {
  vus: 15,
  duration: '3m',
  thresholds: {
    http_req_duration: ['p(95)<1500'],
  },
};

export default function () {
  if (!ADMIN_TOKEN) return;
  const endpoints = [
    '/api/admin/analytics/overview',
    '/api/orders',
    '/api/invoices',
  ];
  for (const path of endpoints) {
    const res = http.get(`${BASE}${path}`, {
      headers: { Authorization: `Bearer ${ADMIN_TOKEN}` },
    });
    check(res, { [`${path} reachable`]: (r) => r.status === 200 || r.status === 403 });
  }
  sleep(1);
}
