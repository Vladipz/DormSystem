# MotivationFake Tests

`MotivationFake` is a fake unstable HTTP service for resilience testing.

Endpoint: `GET /api/motivation/phrase`

Default URL: `http://localhost:5294`

Every request randomly returns one of these configured outcomes:

- fast `200 OK`
- slow `200 OK` after delay
- `500 Internal Server Error`
- `503 Service Unavailable`
- aborted connection

Run the small test:

```bash
k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
```

Run the larger scenario test:

```bash
k6 run Services/MotivationFake/MotivationFake.Tests/Performance/ScenariosGetMotivationPhrase.k6.js
```

Use another host:

```bash
BASE_URL=http://localhost:5294 k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
```
