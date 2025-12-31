import http from 'k6/http';
import { check, sleep } from 'k6';

// Minimal smoke test for latency/scalability.
// Usage:
//   k6 run -e BASE_URL=http://localhost:5000 -e TOKEN="Bearer <jwt>" CoinUpAPI/tests/performance/k6-smoke.js

export const options = {
  vus: 5,
  duration: '10s',
};

const baseUrl = __ENV.BASE_URL || 'http://localhost:5000';
const token = __ENV.TOKEN || '';

export default function () {
  const params = {
    headers: {
      Authorization: token,
    },
  };

  const res = http.get(`${baseUrl}/api/coins?page=1&pageSize=20`, params);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'latency < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
