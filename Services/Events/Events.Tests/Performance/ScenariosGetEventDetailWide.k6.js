import { check, fail } from 'k6';
import http from 'k6/http';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5095';
const SETUP_PAGE_SIZE = 100;

export const options = {
  scenarios: {
    warmup: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [{ duration: '30s', target: 50 }],
      gracefulRampDown: '10s',
      exec: 'detailWarmup',
    },
    normal_load: {
      executor: 'constant-vus',
      vus: 150,
      duration: '2m',
      startTime: '40s',
      exec: 'detailNormal',
    },
    peak_load: {
      executor: 'ramping-vus',
      startTime: '2m40s',
      stages: [
        { duration: '30s', target: 300 },
        { duration: '1m', target: 300 },
        { duration: '20s', target: 100 },
      ],
      gracefulRampDown: '10s',
      exec: 'detailPeak',
    },
    stress_test: {
      executor: 'ramping-vus',
      startTime: '4m40s',
      stages: [
        { duration: '5s', target: 300 },
        { duration: '5s', target: 450 },
        { duration: '5s', target: 600 },
        { duration: '5s', target: 0 },
      ],
      gracefulRampDown: '5s',
      exec: 'detailStress',
    },
  },
  thresholds: {
    checks: ['rate>0.95'],
    http_req_failed: ['rate<0.10'],
    'http_req_duration{scenario:warmup,endpoint:get-event-detail}': ['p(95)<500'],
    'http_req_duration{scenario:normal_load,endpoint:get-event-detail}': ['p(95)<800'],
    'http_req_duration{scenario:peak_load,endpoint:get-event-detail}': ['p(95)<1200'],
    'http_req_duration{scenario:stress_test,endpoint:get-event-detail}': ['p(95)<2000'],
    'http_req_duration{endpoint:get-event-detail}': ['p(95)<1200'],
  },
};

function buildEventsUrl(pageNumber, pageSize) {
  return `${BASE_URL}/api/events?pageNumber=${pageNumber}&pageSize=${pageSize}`;
}

function buildEventDetailUrl(eventId) {
  return `${BASE_URL}/api/events/${eventId}`;
}

export function setup() {
  const eventIds = new Set();
  let pageNumber = 1;
  let totalPages = null;

  while (totalPages === null || pageNumber <= totalPages) {
    const response = http.get(buildEventsUrl(pageNumber, SETUP_PAGE_SIZE), {
      headers: { Accept: 'application/json' },
      tags: {
        feature: 'events',
        endpoint: 'get-events-list',
        phase: 'setup',
        method: 'GET',
      },
    });

    const ok = check(response, {
      'setup list: status 200': (r) => r.status === 200,
      'setup list: json content type': (r) =>
        (r.headers['Content-Type'] || '').includes('application/json'),
    });

    if (!ok) {
      fail(`Setup failed at page ${pageNumber}: status=${response.status}`);
    }

    let body = null;
    try {
      body = response.json();
    } catch {
      fail(`Setup failed: invalid JSON at page ${pageNumber}`);
    }

    if (!body || !Array.isArray(body.items)) {
      fail(`Setup failed: missing items at page ${pageNumber}`);
    }

    if (typeof body.totalPages !== 'number' || body.totalPages < 1) {
      fail(`Setup failed: invalid totalPages at page ${pageNumber}`);
    }

    totalPages = body.totalPages;

    for (const item of body.items) {
      if (item && item.id !== undefined && item.id !== null) {
        eventIds.add(String(item.id));
      }
    }

    pageNumber += 1;
  }

  const allIds = Array.from(eventIds);

  if (allIds.length === 0) {
    fail('Setup failed: no event IDs collected from pagination');
  }

  return { eventIds: allIds };
}

function runDetailBurst(data, burstSize) {
  const ids = data && data.eventIds ? data.eventIds : [];

  if (ids.length === 0) {
    fail('No event IDs available in VU context');
  }

  const startIndex = Math.floor(Math.random() * ids.length);

  for (let i = 0; i < burstSize; i += 1) {
    const idx = (startIndex + i) % ids.length;
    const eventId = ids[idx];

    const response = http.get(buildEventDetailUrl(eventId), {
      headers: { Accept: 'application/json' },
      tags: {
        feature: 'events',
        endpoint: 'get-event-detail',
        flow: 'detail-wide',
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
      'detail: status is 200': (r) => r.status === 200,
      'detail: content type is json': (r) =>
        (r.headers['Content-Type'] || '').includes('application/json'),
      'detail: response is object': () => body !== null && typeof body === 'object',
      'detail: has id': () => body !== null && body.id !== undefined && body.id !== null,
      'detail: id matches requested': () =>
        body !== null && String(body.id) === String(eventId),
    });
  }
}

export function detailWarmup(data) {
  runDetailBurst(data, 10);
}

export function detailNormal(data) {
  runDetailBurst(data, 20);
}

export function detailPeak(data) {
  runDetailBurst(data, 40);
}

export function detailStress(data) {
  runDetailBurst(data, 40);
}

export default function (data) {
  detailNormal(data);
}
