import { check, sleep } from 'k6';
import http from 'k6/http';

const BASE_URL = 'http://localhost:5095';
const PAGE_NUMBER = __ENV.PAGE_NUMBER || '1';
const PAGE_SIZE = __ENV.PAGE_SIZE || '50';

// ⭐ НОВА КОНФІГУРАЦІЯ: progressive load з реалістичним навантаженням
export const options = {
  // Сценарії з різним навантаженням
  scenarios: {
    // Warm-up: поступовий підйом до базового навантаження
    warmup: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 50 },  // 0 → 50 users за 30 сек
      ],
      gracefulRampDown: '10s',
      exec: 'browseEvents',               // виконує функцію browseEvents
    },
    
    // Normal load: базове навантаження (типовий день)
    normal_load: {
      executor: 'constant-vus',
      vus: 150,
      duration: '2m',
      startTime: '40s',                   // починається після warmup
      exec: 'browseEvents',
    },
    
    // Peak load: пікове навантаження (оголошення події)
    peak_load: {
      executor: 'ramping-vus',
      startTime: '2m40s',                 // після normal load
      stages: [
        { duration: '30s', target: 300 }, // різко до 300 users
        { duration: '1m', target: 300 },  // тримаємо 1 хв
        { duration: '20s', target: 100 }, // назад до норми
      ],
      gracefulRampDown: '10s',
      exec: 'browseEventsDetailed',       // під піком більш складні запити
    },
    
    // Stress test: знаходимо breaking point
    stress_test: {
      executor: 'ramping-vus',
      startTime: '4m40s',
      stages: [
        { duration: '5s', target: 300 },
        { duration: '5s', target: 450 },
        { duration: '5s', target: 600 },  // короткий spike наприкінці
        { duration: '5s', target: 0 },
      ],
      gracefulRampDown: '5s',
      exec: 'browseEventsDetailed',
    },
  },
  
  thresholds: {
    checks: ['rate>0.95'],
    
    // Різні пороги для різних навантажень
    'http_req_duration{scenario:warmup}': ['p(95)<500'],
    'http_req_duration{scenario:normal_load}': ['p(95)<800'],
    'http_req_duration{scenario:peak_load}': ['p(95)<1200'],
    'http_req_duration{scenario:stress_test}': ['p(95)<2000'], // під стресом можна повільніше
    
    'http_req_failed': ['rate<0.05'],
    
    // Окремі метрики для GET list vs GET single
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

// ⭐ SCENARIO 1: Простий перегляд списку (легке навантаження)
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

// ⭐ SCENARIO 2: Перегляд списку + детальний перегляд випадкового івенту
export function browseEventsDetailed() {
  // Крок 1: Отримати список подій
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
  
  // Якщо список успішно отримали і є івенти
  if (listChecks && listBody && listBody.items && listBody.items.length > 0) {
    sleep(0.1);
    
    // Крок 2: Вибрати випадковий івент зі списку
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

// ⭐ SCENARIO 3 (опційно): Пагінація - переглянути кілька сторінок
export function browsePagination() {
  const totalPages = 5; // переглянемо 5 сторінок
  
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

// Default function (якщо запускаєте без scenarios)
export default function () {
  browseEventsDetailed();
}

// ⭐ CUSTOM METRICS для детальнішого аналізу
import { Trend } from 'k6/metrics';

const listDuration = new Trend('list_request_duration', true);
const detailDuration = new Trend('detail_request_duration', true);

// Можна додати до функцій для окремого tracking
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
