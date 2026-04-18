import { check, sleep } from 'k6';
import http from 'k6/http';

const BASE_URL = 'http://localhost:5095';
const PAGE_NUMBER = __ENV.PAGE_NUMBER || '1';
const PAGE_SIZE = __ENV.PAGE_SIZE || '50';

// Progressive load configuration with realistic traffic patterns.
export const options = {
  // Scenarios with different load levels.
  scenarios: {
    // Warm-up: gradually ramp to baseline load.
    warmup: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 50 },
      ],
      gracefulRampDown: '10s',
      exec: 'browseEvents',
    },
    
    // Normal load: baseline traffic.
    normal_load: {
      executor: 'constant-vus',
      vus: 150,
      duration: '2m',
      startTime: '40s',
      exec: 'browseEvents',
    },
    
    // Peak load: temporary traffic spike.
    peak_load: {
      executor: 'ramping-vus',
      startTime: '2m40s',
      stages: [
        { duration: '30s', target: 300 },
        { duration: '1m', target: 300 },
        { duration: '20s', target: 100 },
      ],
      gracefulRampDown: '10s',
      exec: 'browseEventsDetailed',
    },
    
    // Stress test: find the breaking point.
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
      exec: 'browseEventsDetailed',
    },
  },
  
  thresholds: {
    checks: ['rate>0.95'],
    
    // Scenario-specific latency thresholds.
    'http_req_duration{scenario:warmup}': ['p(95)<500'],
    'http_req_duration{scenario:normal_load}': ['p(95)<800'],
    'http_req_duration{scenario:peak_load}': ['p(95)<1200'],
    'http_req_duration{scenario:stress_test}': ['p(95)<2000'],
    
    'http_req_failed': ['rate<0.05'],
    
    // Separate thresholds for list and detail endpoints.
    'http_req_duration{endpoint:get-events-list}': ['p(95)<600'],
    'http_req_duration{endpoint:get-event-detail}': ['p(95)<400'],
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

// Scenario 1: list browsing (light workload).
export function browseEvents() {
  const listUrl = buildEventsUrl();
  
  const listResponse = http.get(listUrl, {
    headers: { Accept: 'application/json' },
    tags: {
      feature: 'events',
      endpoint: 'get-events-list',
      method: 'GET',
      flow: 'browse',
    },
  });
  
  let body = null;
  try {
    body = listResponse.json();
  } catch {
    body = null;
  }
  
  check(listResponse, {
    'list: status is 200': (res) => res.status === 200,
    'list: content type is json': (res) =>
      (res.headers['Content-Type'] || '').includes('application/json'),
    'list: response body is an object': () => body !== null && typeof body === 'object',
    'list: response has items array': () => body !== null && Array.isArray(body.items),
    'list: response has pagination': () =>
      body !== null &&
      typeof body.pageNumber === 'number' &&
      typeof body.pageSize === 'number' &&
      typeof body.totalCount === 'number',
  });
  
  sleep(0.1);
}

// Scenario 2: list browsing + random event detail request.
export function browseEventsDetailed() {
  const listUrl = buildEventsUrl();
  
  const listResponse = http.get(listUrl, {
    headers: { Accept: 'application/json' },
    tags: {
      feature: 'events',
      endpoint: 'get-events-list',
      method: 'GET',
      flow: 'detailed',
    },
  });
  
  let listBody = null;
  try {
    listBody = listResponse.json();
  } catch {
    listBody = null;
  }
  
  const listChecks = check(listResponse, {
    'list: status is 200': (res) => res.status === 200,
    'list: has items': () => listBody !== null && Array.isArray(listBody.items),
  });
  
  if (listChecks && listBody && listBody.items && listBody.items.length > 0) {
    sleep(0.1);
    
    const randomIndex = Math.floor(Math.random() * listBody.items.length);
    const randomEvent = listBody.items[randomIndex];
    
    if (randomEvent && randomEvent.id) {
      const detailUrl = buildEventDetailUrl(randomEvent.id);
      
      const detailResponse = http.get(detailUrl, {
        headers: { Accept: 'application/json' },
        tags: {
          feature: 'events',
          endpoint: 'get-event-detail',
          method: 'GET',
          flow: 'detailed',
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
        'detail: response is an object': () => detailBody !== null && typeof detailBody === 'object',
        'detail: has id': () => detailBody !== null && detailBody.id !== undefined,
        'detail: has name': () => detailBody !== null && typeof detailBody.name === 'string',
        'detail: id matches requested': () => detailBody !== null && detailBody.id === randomEvent.id,
      });
      
      sleep(0.1);
    } else {
      sleep(0.1);
    }
  } else {
    sleep(0.1);
  }
}

// Scenario 3 (optional): browse several pages with pagination.
export function browsePagination() {
  const totalPages = 5;
  
  for (let page = 1; page <= totalPages; page++) {
    const url = buildEventsUrl(page, PAGE_SIZE);
    
    const response = http.get(url, {
      headers: { Accept: 'application/json' },
      tags: {
        feature: 'events',
        endpoint: 'get-events-list',
        method: 'GET',
        flow: 'pagination',
        page: String(page),
      },
    });
    
    let body = null;
    try {
      body = response.json();
    } catch {
      body = null;
    }
    
    check(response, {
      [`page ${page}: status is 200`]: (res) => res.status === 200,
      [`page ${page}: has items`]: () => body !== null && Array.isArray(body.items),
      [`page ${page}: page number correct`]: () => body !== null && body.pageNumber === page,
    });
    
    sleep(0.1);
  }
}

// Default run function when executing without explicit scenarios.
export default function () {
  browseEventsDetailed();
}

// Custom metrics for request timing analysis.
import { Trend } from 'k6/metrics';

const listDuration = new Trend('list_request_duration', true);
const detailDuration = new Trend('detail_request_duration', true);

export function browseEventsWithMetrics() {
  const t1 = Date.now();
  const listResponse = http.get(buildEventsUrl());
  listDuration.add(Date.now() - t1);
  
  let listBody = null;
  try {
    listBody = listResponse.json();
  } catch {
    return;
  }
  
  if (listBody && listBody.items && listBody.items.length > 0) {
    sleep(0.1);
    
    const randomEvent = listBody.items[Math.floor(Math.random() * listBody.items.length)];
    
    if (randomEvent && randomEvent.id) {
      const t2 = Date.now();
      http.get(buildEventDetailUrl(randomEvent.id));
      detailDuration.add(Date.now() - t2);
      
      sleep(0.1);
    }
  }
}
