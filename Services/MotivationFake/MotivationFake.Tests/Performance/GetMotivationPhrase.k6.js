import { check, sleep } from 'k6';
import http from 'k6/http';

// MotivationFake is not currently routed through the API Gateway, so the default
// target is the local service URL. Override BASE_URL when running through another host.
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5294';
const SCENARIO = __ENV.SCENARIO || 'ok';

export const options = {
  vus: 5,
  duration: '30s',
  thresholds: {
    checks: ['rate>0.95'],

    // Use SCENARIO=slow to include intentional delay behavior.
    http_req_duration: ['p(95)<6500'],
    http_req_failed: ['rate<0.05'],
  },
};

function buildMotivationPhraseUrl() {
  return `${BASE_URL}/api/motivation/phrase?scenario=${SCENARIO}`;
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
    'status is 200': (res) => res.status === 200,
    'content type is json': (res) =>
      (res.headers['Content-Type'] || '').includes('application/json'),
    'response body is an object': () => body !== null && typeof body === 'object',
    'response has phrase': () => body !== null && typeof body.phrase === 'string',
    'phrase is not empty': () => body !== null && body.phrase.trim().length > 0,
  });

  sleep(1);
}

// Example commands:
// k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
// BASE_URL=http://localhost:5294 SCENARIO=slow k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
