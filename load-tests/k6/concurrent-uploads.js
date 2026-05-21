import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = __ENV.API_BASE_URL || 'http://localhost:5000';
const TOKEN = __ENV.ACCESS_TOKEN || '';

export const options = {
  scenarios: {
    concurrent_uploads: {
      executor: 'constant-vus',
      vus: __ENV.VUS ? parseInt(__ENV.VUS, 10) : 10,
      duration: __ENV.DURATION || '2m',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.1'],
    http_req_duration: ['p(95)<5000'],
  },
};

export default function () {
  const headers = TOKEN
    ? { Authorization: `Bearer ${TOKEN}`, 'X-Requested-With': 'XMLHttpRequest' }
    : { 'X-Requested-With': 'XMLHttpRequest' };

  const res = http.get(`${BASE}/api/health/ready`, { headers });
  check(res, { 'ready ok': (r) => r.status === 200 });
  sleep(0.5);
}
