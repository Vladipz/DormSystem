import { check, fail } from 'k6';
import http from 'k6/http';
import { Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5095';
const SETUP_PAGE_SIZE = __ENV.SETUP_PAGE_SIZE || '100';
const VUS = Number(__ENV.VUS || 10);
const DURATION = __ENV.DURATION || '1m';

const motivationalPhraseEmptyRate = new Rate('motivational_phrase_empty_rate');
const motivationalPhraseNonEmptyRate = new Rate('motivational_phrase_non_empty_rate');

export const options = {
  vus: VUS,
  duration: DURATION,
  thresholds: {
    checks: ['rate>0.95'],
    http_req_failed: ['rate<0.01'],
    'http_req_duration{endpoint:get-event-detail}': ['p(95)<800'],
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

export default function (data) {
  const ids = data && data.eventIds ? data.eventIds : [];
  if (ids.length === 0) {
    fail('No event IDs available in VU context');
  }

  const eventId = ids[Math.floor(Math.random() * ids.length)];
  const response = http.get(buildEventDetailUrl(eventId), {
    headers: { Accept: 'application/json' },
    tags: {
      feature: 'events',
      endpoint: 'get-event-detail',
      flow: 'detail-simple',
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
    'detail: participants is array': () =>
      body !== null && Array.isArray(body.participants),
    'detail: motivational phrase is string': () =>
      body !== null && typeof body.motivationalPhrase === 'string',
  });

  if (body !== null && typeof body.motivationalPhrase === 'string') {
    const isEmpty = body.motivationalPhrase.length === 0;
    motivationalPhraseEmptyRate.add(isEmpty);
    motivationalPhraseNonEmptyRate.add(!isEmpty);
  }
}
