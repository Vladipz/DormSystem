# MotivationFake

Fake unstable HTTP service used to test Events resilience behavior.

Endpoint: `GET /api/motivation/phrase`

Default behavior per request:

| Result | Chance | Notes |
|---|---:|---|
| Fast `200 OK` | 60% | Returns a phrase |
| Slow `200 OK` | 20% | Waits `5000ms`, then returns a phrase |
| `500 Internal Server Error` | 10% | Simulates server failure |
| `503 Service Unavailable` | 5% | Simulates unavailable dependency |
| Aborted connection | 5% | Simulates dropped HTTP connection |

Docker/env overrides:

- `MOTIVATION_FAKE_SLOW_DELAY_MS`
- `MOTIVATION_FAKE_PROBABILITY_SLOW`
- `MOTIVATION_FAKE_PROBABILITY_ERROR`
- `MOTIVATION_FAKE_PROBABILITY_UNAVAILABLE`
- `MOTIVATION_FAKE_PROBABILITY_ABORT`

Events calls this service when loading event details. Failed calls are handled by `MotivationFakeClient` and become an empty `motivationalPhrase`.
