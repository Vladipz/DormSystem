# Notification Delivery Latency Experiment Plan

## Goal

Measure and compare how long it takes for an in-app notification to reach an opened frontend client in two delivery modes:

- `websocket`
- `polling_5s`

The measured latency is:

- `client_ack_time - Notification.CreatedAt`

This experiment is intentionally simple.

Important constraints:

- count background tabs too
- do not measure closed/offline clients
- keep implementation lightweight
- do not add unnecessary persistence or complex correlation logic
- tests will be run separately for each mode, not at the same time
- manual test execution is out of scope; implementation only

## Current Implementation Status

### Done

- custom histogram metric plan implemented in codebase
- receipt endpoint implemented
- polling changes endpoint implemented
- frontend delivery mode env added
- websocket receipt acknowledgements implemented
- polling listener and unified delivery listener implemented
- two dedicated seeded test students planned and implemented in auth seeding
- notification seed alignment implemented, including in-app channels for test users
- test notification generator endpoint implemented
- test notification generator endpoint made public for local experiments
- helper shell script added for 100 sequential test notifications over about one minute

### Remaining

- manual runtime verification by the user
- metric visualization/filtering in Aspire dashboard or another OTel consumer

---

## What Will Be Measured

### Start point

Use `Notification.CreatedAt` from `NotificationCore` as the start timestamp.

Existing file:
- `Services/NotificationCore/NotificationCore.API/Entities/Notification.cs`

Reason:
- already exists
- stable
- directly tied to in-app notification delivery

### End point

Use a frontend receipt acknowledgement sent immediately after the client receives and processes the notification in UI logic.

This means:
- SignalR callback received notification -> frontend updates UI/cache -> frontend sends receipt
- polling response received notification -> frontend updates UI/cache -> frontend sends receipt

### Final metric

One custom histogram metric only:

- `notifications.delivery_latency_ms`

One tag only:

- `mode=websocket|polling_5s`

No `type` tag is needed.

Reason:
- the goal is to compare delivery mechanism latency, not notification categories
- fewer labels means simpler metrics and cleaner charts

---

## Expected Interpretation

The metric is valid for:

- active opened frontend client
- visible tab
- background tab

The metric is not intended for:

- closed tabs
- offline users
- "time until human reads the message"

This is an online in-app delivery latency metric.

---

## Visualization Strategy

There will be one histogram metric in the system:

- `notifications.delivery_latency_ms`

In dashboards, compare it by filtering or grouping on `mode`:

- `mode=websocket`
- `mode=polling_5s`

This allows viewing:

- `avg`
- `p50`
- `p95`

Even when tests are run separately.

Example interpretation:

- websocket: low avg/p50/p95
- polling_5s: avg around half of polling interval plus overhead, p95 near interval ceiling

---

## Backend Implementation Plan

## 1. Add custom notification metric

### New file

- `Services/NotificationCore/NotificationCore.API/Observability/NotificationMetrics.cs`

### Purpose

Define a dedicated meter and histogram for notification delivery latency.

### Important code shape

```csharp
using System.Diagnostics.Metrics;

namespace NotificationCore.API.Observability;

public static class NotificationMetrics
{
    public const string MeterName = "DormSystem.Notifications";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Histogram<double> DeliveryLatencyMs =
        Meter.CreateHistogram<double>(
            "notifications.delivery_latency_ms",
            unit: "ms",
            description: "Latency from notification creation to client receipt ack");
}
```

### Notes

- keep just one histogram
- no extra counters unless later needed

---

## 2. Register custom meter in OpenTelemetry

### Existing file

- `Services/Shared/ServiceDefaults/Extensions.cs`

### Required change

Extend metrics registration to include the custom notification meter.

### Important code shape

```csharp
metrics.AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddRuntimeInstrumentation()
    .AddMeter("DormSystem.Notifications");
```

### Notes

- this allows Aspire / OTLP exporters to capture the custom histogram

---

## 3. Add receipt endpoint

### New file

- `Services/NotificationCore/NotificationCore.API/Features/Notifications/RegisterNotificationReceipt.cs`

### Route

- `POST /api/notifications/me/receipt`

### Request contract

```json
{
  "notificationId": "guid",
  "mode": "websocket",
  "receivedAtUtc": "2026-03-25T12:00:01.200Z"
}
```

### Behavior

- get current user from JWT
- find notification by `notificationId`
- verify notification belongs to current user
- compute latency from `notification.CreatedAt`
- record histogram with tag `mode`
- return `204 No Content`

### Important code shape

```csharp
public sealed record Request(
    Guid NotificationId,
    string Mode,
    DateTime ReceivedAtUtc);
```

```csharp
var notification = await _db.Notifications
    .FirstOrDefaultAsync(
        x => x.Id == request.NotificationId && x.UserId == request.UserId,
        cancellationToken);

if (notification is null)
{
    return Results.NotFound();
}

var latencyMs = (request.ReceivedAtUtc - notification.CreatedAt).TotalMilliseconds;

if (latencyMs >= 0)
{
    NotificationMetrics.DeliveryLatencyMs.Record(
        latencyMs,
        new KeyValuePair<string, object?>("mode", request.Mode));
}
```

### Notes

- no database table for receipts
- no receipt persistence
- no complex dedup logic at this stage

---

## 4. Add simple polling changes endpoint

### New file

- `Services/NotificationCore/NotificationCore.API/Features/Notifications/GetMyNotificationChanges.cs`

### Route

- `GET /api/notifications/me/changes?afterCreatedAt=...&limit=50`

### Purpose

Return notifications created after a known client timestamp for use in polling mode.

### Response shape

```json
{
  "notifications": [ ... ]
}
```

### Important code shape

```csharp
var notifications = await _db.Notifications
    .Where(x => x.UserId == request.UserId)
    .Where(x => x.CreatedAt > request.AfterCreatedAtUtc)
    .OrderBy(x => x.CreatedAt)
    .Take(limit)
    .Select(x => new GetMyNotifications.NotificationDto(
        x.Id,
        x.Title,
        x.Message,
        x.Type,
        x.CreatedAt,
        x.IsRead))
    .ToListAsync(cancellationToken);
```

### Notes

- keep this endpoint minimal
- do not add complicated cursor logic now
- `afterCreatedAt` is enough for the experiment

---

## 5. Backend registration impact

### Existing file

- `Services/NotificationCore/NotificationCore.API/Program.cs`

### Notes

- Carter should auto-pick up the new notification features
- SignalR registration already exists
- no major structural changes are expected beyond new feature files

---

## Frontend Implementation Plan

## 1. Add delivery mode env config

### Existing file

- `frontend/dorm-app/.env.example`

### Required addition

```env
VITE_API_GATEWAY_URL=http://localhost:5095
VITE_NOTIFICATIONS_DELIVERY_MODE=websocket
```

### Allowed values

- `websocket`
- `polling_5s`

### Notes

- tests will be run separately by changing this mode
- no simultaneous dual transport is needed

---

## 2. Extend notification frontend types

### Existing file

- `frontend/dorm-app/src/lib/types/notification.ts`

### Required additions

```ts
export type NotificationDeliveryMode = "websocket" | "polling_5s";

export interface RegisterNotificationReceiptRequest {
  notificationId: string;
  mode: NotificationDeliveryMode;
  receivedAtUtc: string;
}

export interface NotificationChangesResponse {
  notifications: UserNotification[];
}
```

### Notes

- keep mode names explicit and simple

---

## 3. Extend notification service

### Existing file

- `frontend/dorm-app/src/lib/services/notificationService.ts`

### Required additions

```ts
async registerReceipt(payload: RegisterNotificationReceiptRequest) {
  return api.post<void>("/notifications/me/receipt", payload);
}
```

```ts
async getNotificationChanges(afterCreatedAt?: string, limit = 50) {
  return api.get<NotificationChangesResponse>("/notifications/me/changes", {
    afterCreatedAt,
    limit,
  });
}
```

### Notes

- reuse current gateway-based API layer
- no new client instance is needed

---

## 4. Update SignalR listener for websocket mode

### Existing file

- `frontend/dorm-app/src/components/NotificationRealtimeListener.tsx`

### Required behavior changes

- read `VITE_NOTIFICATIONS_DELIVERY_MODE`
- if mode is not `websocket`, do not start SignalR connection
- after notification is received and processed, send receipt

### Important code shape

```ts
const DELIVERY_MODE =
  (import.meta.env.VITE_NOTIFICATIONS_DELIVERY_MODE as NotificationDeliveryMode | undefined)
  ?? "websocket";

if (DELIVERY_MODE !== "websocket") {
  return;
}
```

### Receipt logic after callback processing

```ts
void notificationService.registerReceipt({
  notificationId: notification.id,
  mode: "websocket",
  receivedAtUtc: new Date().toISOString(),
});
```

### Notes

- send receipt after toast/cache update, not before
- background tab behavior is still valid because the page remains open and callback still runs

---

## 5. Add polling listener

### New file

- `frontend/dorm-app/src/components/NotificationPollingListener.tsx`

### Purpose

Run a simple 5-second polling loop when delivery mode is `polling_5s`.

### Required behavior

- start only if user is authenticated
- start only if mode is `polling_5s`
- every 5 seconds call `getNotificationChanges(...)`
- update notification cache with new notifications
- show toast for each new notification
- send receipt for each new notification

### Important code shape

```ts
const POLL_INTERVAL_MS = 5000;
const lastSeenCreatedAtRef = useRef<string | null>(null);
```

```ts
const response = await notificationService.getNotificationChanges(
  lastSeenCreatedAtRef.current ?? undefined,
  50,
);
```

```ts
for (const notification of response.notifications) {
  // toast
  // cache update
  // receipt
}
```

```ts
void notificationService.registerReceipt({
  notificationId: notification.id,
  mode: "polling_5s",
  receivedAtUtc: new Date().toISOString(),
});
```

### Notes

- keep polling logic extremely simple
- no backoff logic needed initially
- no complex duplicate filtering beyond existing cache checks

---

## 6. Add unified delivery listener wrapper

### New file

- `frontend/dorm-app/src/components/NotificationDeliveryListener.tsx`

### Purpose

Choose one delivery mechanism based on env mode.

### Important code shape

```tsx
export function NotificationDeliveryListener() {
  const mode = notificationDeliveryMode;

  if (mode === "polling_5s") {
    return <NotificationPollingListener />;
  }

  return <NotificationRealtimeListener />;
}
```

### Notes

- only one mechanism should be active at a time

---

## 7. Replace main layout listener mount

### Existing file

- `frontend/dorm-app/src/routes/_mainLayout.tsx`

### Change

Replace:

```tsx
<NotificationRealtimeListener />
```

With:

```tsx
<NotificationDeliveryListener />
```

### Notes

- this keeps the rest of the UI unchanged

---

## Cache Strategy

Reuse the current notifications cache:

- `myNotifications`

Existing related files:
- `frontend/dorm-app/src/lib/hooks/useNotification.ts`
- `frontend/dorm-app/src/components/NotificationRealtimeListener.tsx`

The same cache update pattern should be used for polling mode.

No separate cache architecture is needed.

---

## File Checklist

### Backend

- `Services/NotificationCore/NotificationCore.API/Observability/NotificationMetrics.cs` (new)
- `Services/Shared/ServiceDefaults/Extensions.cs` (update)
- `Services/NotificationCore/NotificationCore.API/Features/Notifications/RegisterNotificationReceipt.cs` (new)
- `Services/NotificationCore/NotificationCore.API/Features/Notifications/GetMyNotificationChanges.cs` (new)
- `Services/NotificationCore/NotificationCore.API/Program.cs` (only if minor registration changes are needed)

### Frontend

- `frontend/dorm-app/.env.example` (update)
- `frontend/dorm-app/src/lib/types/notification.ts` (update)
- `frontend/dorm-app/src/lib/services/notificationService.ts` (update)
- `frontend/dorm-app/src/components/NotificationRealtimeListener.tsx` (update)
- `frontend/dorm-app/src/components/NotificationPollingListener.tsx` (new)
- `frontend/dorm-app/src/components/NotificationDeliveryListener.tsx` (new)
- `frontend/dorm-app/src/routes/_mainLayout.tsx` (update)

---

## Metrics Output Goal

After implementation, the system should support viewing:

- `avg(notifications.delivery_latency_ms)` filtered by `mode`
- `p50(notifications.delivery_latency_ms)` filtered by `mode`
- `p95(notifications.delivery_latency_ms)` filtered by `mode`

This should produce a clear comparison between:

- `websocket`
- `polling_5s`

using separate test runs.

---

## Out Of Scope

- storing receipts in the database
- source-event correlation across multiple services
- notification-type-based metric segmentation
- simultaneous dual delivery mode testing
- manual test execution by the coding agent
- performance automation or benchmark orchestration

---

## Recommended Execution Order

1. Add `NotificationMetrics`
2. Register custom meter in OpenTelemetry
3. Add receipt endpoint
4. Add polling changes endpoint
5. Extend frontend notification types and service
6. Update SignalR listener to send receipts in websocket mode
7. Add polling listener
8. Add delivery listener wrapper
9. Replace main layout mount point
10. Update `.env.example`

This order keeps implementation incremental and easy to reason about.
