import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = __ENV.API_BASE_URL || 'http://localhost:5000';
const TOKEN = __ENV.ADMIN_TOKEN || '';

export const options = {
  scenarios: {
    invoice_list_spike: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 15 },
        { duration: '1m', target: 30 },
        { duration: '30s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<3000'],
  },
};

export default function () {
  if (!TOKEN) {
    sleep(1);
    return;
  }

  const headers = {
    Authorization: `Bearer ${TOKEN}`,
    'Content-Type': 'application/json',
    'X-Requested-With': 'XMLHttpRequest',
  };

  const res = http.get(`${BASE}/api/invoices?page=1&pageSize=50`, { headers });
  check(res, { 'invoices 200': (r) => r.status === 200 });
  sleep(1);
}
