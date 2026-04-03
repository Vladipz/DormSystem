import { check } from 'k6';
import http from 'k6/http';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5095';
const PAGE_NUMBER = __ENV.PAGE_NUMBER || '1';
const PAGE_SIZE = __ENV.PAGE_SIZE || '100';
const DURATION = __ENV.DURATION || '1m';
const VUS = Number(__ENV.VUS || '700');

export const options = {
  scenarios: {
    brutal_events: {
      executor: 'constant-vus',
      vus: VUS,
      duration: DURATION,
      exec: 'hammerEvents',
    },
  },
  thresholds: {
    checks: ['rate>0.90'],
    http_req_failed: ['rate<0.10'],
    'http_req_duration{endpoint:get-events-list}': ['p(95)<1500'],
    'http_req_duration{endpoint:get-event-detail}': ['p(95)<1500'],
  },
};

function buildEventsUrl(pageNumber = PAGE_NUMBER, pageSize = PAGE_SIZE) {
  const query =
    `pageNumber=${encodeURIComponent(pageNumber)}` +
    `&pageSize=${encodeURIComponent(pageSize)}`;

  return `${BASE_URL}/api/events?${query}`;
}

function buildEventDetailUrl(eventId) {
  return `${BASE_URL}/api/events/${eventId}`;
}

export function hammerEvents() {
  const listResponse = http.get(buildEventsUrl(), {
    headers: { Accept: 'application/json' },
    tags: {
      feature: 'events',
      endpoint: 'get-events-list',
      method: 'GET',
      flow: 'brutal',
    },
  });

  let listBody = null;
  try {
    listBody = listResponse.json();
  } catch {
    listBody = null;
  }

  const hasItems =
    listBody !== null &&
    Array.isArray(listBody.items) &&
    listBody.items.length > 0;

  check(listResponse, {
    'list: status is 200': (res) => res.status === 200,
    'list: content type is json': (res) =>
      (res.headers['Content-Type'] || '').includes('application/json'),
    'list: has items': () => hasItems,
  });

  if (!hasItems) {
    return;
  }

  const randomEvent = listBody.items[Math.floor(Math.random() * listBody.items.length)];
  if (!randomEvent || !randomEvent.id) {
    return;
  }

  const detailResponse = http.get(buildEventDetailUrl(randomEvent.id), {
    headers: { Accept: 'application/json' },
    tags: {
      feature: 'events',
      endpoint: 'get-event-detail',
      method: 'GET',
      flow: 'brutal',
    },
  });

  let detailBody = null;
  try {
    detailBody = detailResponse.json();
  } catch {
    detailBody = null;
  }

  check(detailResponse, {
    'detail: status is 200': (res) => res.status === 200,
    'detail: content type is json': (res) =>
      (res.headers['Content-Type'] || '').includes('application/json'),
    'detail: has id': () => detailBody !== null && detailBody.id !== undefined,
    'detail: has name': () => detailBody !== null && typeof detailBody.name === 'string',
    'detail: id matches requested': () => detailBody !== null && detailBody.id === randomEvent.id,
  });
}

export default function () {
  hammerEvents();
}
