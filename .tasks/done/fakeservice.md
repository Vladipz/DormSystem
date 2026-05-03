# Технічне завдання — Сервіс MotivationFake

**Версія:** 1.0  
**Проєкт:** InDorm / DeltaM  
**Статус:** Draft  
**Дата:** 03.05.2026

---

## 1. Призначення

`MotivationFake` — синтетичний HTTP-сервіс, що імітує поведінку нестабільної зовнішньої залежності. Призначений виключно для **тестування resilience-механізмів** (Retry, Circuit Breaker, Timeout, Fallback) у сервісах-споживачах. У продакшн не розгортається.

---

## 2. Контекст використання

Сервіс підключається у тестовому середовищі замість реального upstream-сервісу. Споживач (наприклад, сервіс підбору мотиваційних фраз для мешканців гуртожитку) звертається до `MotivationFake` і має коректно обробляти всі типи відмов, описані нижче.

```
[Споживач] ──HTTP──► [MotivationFake]
                           │
                     імітує відмови
```

---

## 3. Ендпоінт

### `GET /api/motivation/phrase`

Повертає мотиваційну фразу або імітує відмову відповідно до розподілу сценаріїв.

#### Успішна відповідь (200 OK)
```json
{
  "phrase": "Keep going, your next event can be the best one."
}
```

#### Параметри запиту (опціонально)

| Параметр   | Тип    | Опис                                                       |
|------------|--------|------------------------------------------------------------|
| `scenario` | string | Примусовий вибір сценарію: `ok`, `slow`, `error`, `unavailable`, `abort` |

Якщо `scenario` не передано — сценарій обирається випадково відповідно до розподілу (розділ 4).

---

## 4. Розподіл сценаріїв (random mode)

| Пріоритет | Сценарій           | Імовірність | Поведінка                                  |
|-----------|--------------------|-------------|--------------------------------------------|
| 1         | `slow`             | 20%         | Затримка 5000 мс, потім 200 OK             |
| 2         | `error`            | 10%         | Відповідь 500 Internal Server Error        |
| 3         | `unavailable`      | 5%          | Відповідь 503 Service Unavailable          |
| 4         | `abort`            | 5%          | Обрив TCP-з'єднання (`HttpContext.Abort()`) |
| 5         | `ok`               | 60%         | Негайна відповідь 200 OK                   |

> **Обґрунтування розподілу:** 60% нормальних відповідей дозволяє спостерігати повний цикл Circuit Breaker — відкриття та закриття — без постійного перебування у відкритому стані.

---

## 5. Детальний опис сценаріїв

### 5.1 `ok` — нормальна відповідь
- HTTP 200
- Тіло: `{ "phrase": "<рядок>" }`
- Фраза обирається випадково з фіксованого набору (≥ 4 варіанти)
- Latency: < 50 мс

### 5.2 `slow` — повільна відповідь
- Затримка: 5000 мс (`Task.Delay`)
- HTTP 200 після затримки
- Тіло: аналогічне до `ok`
- **Мета:** активувати Timeout policy на споживачі

### 5.3 `error` — серверна помилка
- HTTP 500
- Тіло: `ProblemDetails` з повідомленням `"Upstream dependency failed"`
- **Мета:** активувати Retry policy та Circuit Breaker

### 5.4 `unavailable` — сервіс недоступний
- HTTP 503
- Тіло: `ProblemDetails` з повідомленням `"Service temporarily unavailable"`
- **Мета:** активувати Retry з exponential backoff та Circuit Breaker

### 5.5 `abort` — обрив з'єднання
- `HttpContext.Abort()` без відповіді
- Споживач отримає `IOException` / `TaskCanceledException`
- **Мета:** перевірити обробку мережевих збоїв та Fallback

---

## 6. Технічний стек

| Компонент       | Технологія                          |
|-----------------|-------------------------------------|
| Runtime         | .NET 8 / ASP.NET Core Minimal API   |
| Service defaults| `builder.AddServiceDefaults()`      |
| Документація    | OpenAPI (тільки в Development)      |
| Контейнеризація | Docker (dev-оточення)               |

---

## 7. Конфігурація

```json
// appsettings.json
{
  "MotivationFake": {
    "SlowDelayMs": 5000,
    "Probabilities": {
      "Slow": 0.20,
      "Error": 0.10,
      "Unavailable": 0.05,
      "Abort": 0.05
    }
  }
}
```

Значення мають зчитуватись через `IOptions<MotivationFakeOptions>` для можливості зміни без перекомпіляції.

---

## 8. Реалізація (псевдокод)

```csharp
app.MapGet("/api/motivation/phrase", async (
    HttpContext httpContext,
    IOptions<MotivationFakeOptions> opts,
    string? scenario,
    CancellationToken cancellationToken) =>
{
    var cfg = opts.Value;
    var active = scenario ?? ResolveRandom(cfg.Probabilities);

    return active switch
    {
        "slow" => await SlowResponseAsync(phrases, cfg.SlowDelayMs, cancellationToken),
        "error" => Results.Problem("Upstream dependency failed", statusCode: 500),
        "unavailable" => Results.Problem("Service temporarily unavailable", statusCode: 503),
        "abort" => Abort(httpContext),
        _ => Results.Ok(new { phrase = phrases[Random.Shared.Next(phrases.Length)] })
    };
});
```

---

## 9. Обмеження та нефункціональні вимоги

- Сервіс **не має** стану між запитами — кожен запит незалежний
- Не використовується база даних або зовнішні залежності
- Час запуску: < 3 с
- Не підключається до API Gateway у тестовому середовищі — звернення напряму
- Порт за замовчуванням: `5294` (конфігурується через `launchSettings.json`)

---

## 10. Перевірка (k6)

Наявний тест `ScenariosGetMotivationPhrase.k6.js` перевіряє коректність роботи самого `MotivationFake`. Сценарії навантаження:

| Сценарій       | VUs  | Тривалість | p(95) threshold |
|----------------|------|------------|-----------------|
| `warmup`       | 20   | 30s        | < 6500 мс       |
| `normal_load`  | 50   | 2m         | < 7000 мс       |
| `peak_load`    | 100  | 1m50s      | < 8000 мс       |
| `stress_test`  | 200  | 40s        | < 10000 мс      |

Тест вважається пройденим, якщо:
- `checks rate > 95%`
- `http_req_failed rate < 5%`
- усі threshold по `p(95)` виконані

---

## 11. Що НЕ входить у scope

- Авторизація / автентифікація
- Персистентність даних
- Метрики (Prometheus/OpenTelemetry) — опціонально, не обов'язково
- Розгортання у staging/production
