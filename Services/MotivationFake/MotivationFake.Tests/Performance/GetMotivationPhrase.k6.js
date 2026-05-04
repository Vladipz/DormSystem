import { check, sleep } from 'k6';
import http from 'k6/http';

// MotivationFake is not currently routed through the API Gateway, so the default
// target is the local service URL. Override BASE_URL when running through another host.
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5294';

export const options = {
  vus: 5,
  duration: '30s',
  thresholds: {
    checks: ['rate>0.95'],

    http_req_duration: ['p(95)<6500'],
    // Default config intentionally returns error/unavailable/abort about 20% of the time.
    http_req_failed: ['rate<0.25'],
  },
};

function buildMotivationPhraseUrl() {
  return `${BASE_URL}/api/motivation/phrase`;
}

export default function () {
  const response = http.get(buildMotivationPhraseUrl(), {
    headers: {
      Accept: 'application/json',
    },
    tags: {
      feature: 'motivation-fake',
      endpoint: 'get-motivation-phrase',
      method: 'GET',
    },
  });

  let body = null;
  try {
    body = response.json();
  } catch {
    body = null;
  }

  check(response, {
    'status is expected': (res) => [0, 200, 500, 503].includes(res.status),
    'successful response is json': (res) =>
      res.status !== 200 || (res.headers['Content-Type'] || '').includes('application/json'),
    'successful response has phrase': (res) =>
      res.status !== 200 || (body !== null && typeof body.phrase === 'string'),
    'successful phrase is not empty': (res) =>
      res.status !== 200 || body.phrase.trim().length > 0,
  });

  sleep(1);
}

// Example commands:
// k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
// BASE_URL=http://localhost:5294 k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
