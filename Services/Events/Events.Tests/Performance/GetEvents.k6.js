import { check, sleep } from 'k6';
import http from 'k6/http';

// This is the first k6 test in the repository, so the defaults are intentionally small
// and safe. The goal is to verify the endpoint works under light load before adding
// heavier scenarios such as ramp-up, stress, or soak tests.

// This script only targets the API Gateway.
// Keep the gateway running locally before starting the test.
const BASE_URL = 'http://localhost:5095';

// Keep pagination configurable so you can reuse the same script with different data sizes.
const PAGE_NUMBER = __ENV.PAGE_NUMBER || '1';
const PAGE_SIZE = __ENV.PAGE_SIZE || '10';

// k6 options describe how the load test behaves.
// This first version uses a simple fixed-VU scenario:
// - 5 virtual users
// - for 30 seconds
// This is enough to validate the endpoint and collect baseline latency metrics.
export const options = {
  vus: 5,
  duration: '30s',
  thresholds: {
    // At least 95% of all requests should pass our checks.
    checks: ['rate>0.95'],

    // 95% of requests should finish in under 800ms.
    // This is a reasonable first threshold, not a strict final SLA.
    http_req_duration: ['p(95)<800'],

    // Fewer than 5% of requests should fail at the HTTP layer.
    http_req_failed: ['rate<0.05'],
  },
};

function buildEventsUrl() {
  // k6 does not provide all browser URL helpers, so build the query string manually.
  const query =
    `pageNumber=${encodeURIComponent(PAGE_NUMBER)}` +
    `&pageSize=${encodeURIComponent(PAGE_SIZE)}`;

  return `${BASE_URL}/api/events?${query}`;
}

export default function () {
  // The default function is executed once per iteration by each virtual user.
  // In this script, one iteration means one GET request to the Events list endpoint.
  const url = buildEventsUrl();

  const response = http.get(url, {
    headers: {
      Accept: 'application/json',
    },

    // These tags help when reading k6 output or extending the script later.
    tags: {
      feature: 'events',
      endpoint: 'get-events',
      method: 'GET',
    },
  });

  // We parse JSON once and reuse it inside checks.
  // If the body is not valid JSON, we keep null and let checks report the failure.
  let body = null;
  try {
    body = response.json();
  } catch {
    body = null;
  }

  check(response, {
    // Basic transport-level expectation.
    'status is 200': (res) => res.status === 200,

    // Confirms the API returns JSON instead of HTML or plain text.
    'content type is json': (res) =>
      (res.headers['Content-Type'] || '').includes('application/json'),

    // The endpoint returns a paginated response, so the root should be an object.
    'response body is an object': () => body !== null && typeof body === 'object',

    // `items` is the main payload for the event list.
    'response has items array': () => body !== null && Array.isArray(body.items),

    // These fields come from the shared paged response model used by the API.
    'response has page number': () => body !== null && typeof body.pageNumber === 'number',
    'response has page size': () => body !== null && typeof body.pageSize === 'number',
    'response has total count': () => body !== null && typeof body.totalCount === 'number',
    'response has total pages': () => body !== null && typeof body.totalPages === 'number',
    'response has navigation flags': () =>
      body !== null &&
      typeof body.hasPreviousPage === 'boolean' &&
      typeof body.hasNextPage === 'boolean',
  });

  // A short sleep prevents each virtual user from hammering the endpoint in a tight loop.
  // This makes the first baseline test easier to reason about.
  sleep(1);
}

// Example commands:
// k6 run Services/Events/Events.Tests/Performance/GetEvents.k6.js
// PAGE_SIZE=25 k6 run Services/Events/Events.Tests/Performance/GetEvents.k6.js
