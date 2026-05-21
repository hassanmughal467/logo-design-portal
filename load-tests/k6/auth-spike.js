import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = __ENV.API_BASE_URL || 'http://localhost:5000';

export const options = {
  scenarios: {
    auth_spike: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 20 },
        { duration: '1m', target: 50 },
        { duration: '30s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<2000'],
  },
};

export default function () {
  const res = http.post(
    `${BASE}/api/auth/login`,
    JSON.stringify({ email: 'client@test.com', password: 'Test@123' }),
    { headers: { 'Content-Type': 'application/json' } }
  );
  check(res, { 'login status 200 or 401': (r) => r.status === 200 || r.status === 401 });
  sleep(1);
}
