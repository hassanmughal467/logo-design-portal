import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = __ENV.API_BASE_URL || 'http://localhost:5000';
const TOKEN = __ENV.K6_CLIENT_TOKEN || '';

export const options = {
  vus: 10,
  duration: '2m',
  thresholds: {
    http_req_failed: ['rate<0.1'],
    http_req_duration: ['p(95)<3000'],
  },
};

export default function () {
  if (!TOKEN) {
    console.warn('Set K6_CLIENT_TOKEN from a valid client login');
    return;
  }
  const res = http.post(
    `${BASE}/api/orders`,
    JSON.stringify({
      title: `k6 order ${__VU}-${__ITER}`,
      description: 'Load test order with sufficient description length for validation.',
      price: 10,
    }),
    {
      headers: {
        Authorization: `Bearer ${TOKEN}`,
        'Content-Type': 'application/json',
      },
    }
  );
  check(res, { 'create order ok or rate limited': (r) => [200, 201, 429].includes(r.status) });
  sleep(0.5);
}
