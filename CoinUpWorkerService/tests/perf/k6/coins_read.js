import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 5,
  duration: '30s',
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5269';
const EMAIL = __ENV.EMAIL || '';
const PASSWORD = __ENV.PASSWORD || '';

function login() {
  if (!EMAIL || !PASSWORD) {
    return null;
  }

  const url = `${BASE_URL}/api/auth/login`;
  const payload = JSON.stringify({ email: EMAIL, password: PASSWORD });
  const params = { headers: { 'Content-Type': 'application/json' } };

  const res = http.post(url, payload, params);
  const ok = check(res, {
    'login status is 200': (r) => r.status === 200,
  });

  if (!ok) {
    return null;
  }

  try {
    const body = res.json();
    // Your API likely returns something like { token: "..." }
    return body.token || body.accessToken || null;
  } catch (_) {
    return null;
  }
}

export default function () {
  const token = login();

  const headers = token
    ? { Authorization: `Bearer ${token}` }
    : {};

  const res = http.get(`${BASE_URL}/api/coins?page=1&pageSize=20`, { headers });

  check(res, {
    'coins status is 200 or 401': (r) => r.status === 200 || r.status === 401,
  });

  sleep(1);
}
