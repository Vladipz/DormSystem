import { check, sleep } from 'k6';
import http from 'k6/http';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5294';
const SCENARIO = __ENV.SCENARIO || 'ok';

export const options = {
  scenarios: {
    warmup: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 20 },
      ],
      gracefulRampDown: '10s',
      exec: 'getMotivationPhrase',
    },

    normal_load: {
      executor: 'constant-vus',
      vus: 50,
      duration: '2m',
      startTime: '40s',
      exec: 'getMotivationPhrase',
    },

    peak_load: {
      executor: 'ramping-vus',
      startTime: '2m40s',
      stages: [
        { duration: '30s', target: 100 },
        { duration: '1m', target: 100 },
        { duration: '20s', target: 25 },
      ],
      gracefulRampDown: '10s',
      exec: 'getMotivationPhrase',
    },

    stress_test: {
      executor: 'ramping-vus',
      startTime: '4m40s',
      stages: [
        { duration: '10s', target: 100 },
        { duration: '10s', target: 150 },
        { duration: '10s', target: 200 },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '5s',
      exec: 'getMotivationPhrase',
    },
  },

  thresholds: {
    checks: ['rate>0.95'],
    http_req_failed: ['rate<0.05'],

    // A delayed response path is part of this fake service behavior.
    'http_req_duration{scenario:warmup}': ['p(95)<6500'],
    'http_req_duration{scenario:normal_load}': ['p(95)<7000'],
    'http_req_duration{scenario:peak_load}': ['p(95)<8000'],
    'http_req_duration{scenario:stress_test}': ['p(95)<10000'],
    'http_req_duration{endpoint:get-motivation-phrase}': ['p(95)<8000'],
  },
};

function buildMotivationPhraseUrl() {
  return `${BASE_URL}/api/motivation/phrase?scenario=${SCENARIO}`;
}

export function getMotivationPhrase() {
  const response = http.get(buildMotivationPhraseUrl(), {
    headers: { Accept: 'application/json' },
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

  sleep(0.2);
}

export default function () {
  getMotivationPhrase();
}

// Example command:
// k6 run Services/MotivationFake/MotivationFake.Tests/Performance/ScenariosGetMotivationPhrase.k6.js
// BASE_URL=http://localhost:5294 SCENARIO=slow k6 run Services/MotivationFake/MotivationFake.Tests/Performance/ScenariosGetMotivationPhrase.k6.js
