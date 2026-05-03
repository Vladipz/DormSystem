# MotivationFake Performance Tests

This folder contains `k6` performance tests for the MotivationFake API.

MotivationFake is not currently routed through `Services/ApiGateway/appsettings.json`, so these scripts default to the direct local API URL.

- Default local service URL: `http://localhost:5294`
- Endpoint under test: `GET /api/motivation/phrase`

## Scripts

- `GetMotivationPhrase.k6.js` - small baseline smoke/load test.
- `ScenariosGetMotivationPhrase.k6.js` - warm-up, normal load, peak load, and stress scenarios.

## Run

Start the MotivationFake API first, then run one of the scripts:

```bash
k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
```

```bash
k6 run Services/MotivationFake/MotivationFake.Tests/Performance/ScenariosGetMotivationPhrase.k6.js
```

Override the target host with `BASE_URL`:

```bash
BASE_URL=http://localhost:5294 k6 run Services/MotivationFake/MotivationFake.Tests/Performance/GetMotivationPhrase.k6.js
```

## Notes

The API intentionally delays about 30% of responses by 5 seconds. The latency thresholds account for that behavior and should not be interpreted as final production SLAs.
