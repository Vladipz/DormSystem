# In-App Notifications via SignalR Plan

## Goal

Add client-side in-app notifications to DormSystem using SignalR.

This plan treats `InApp` as an internal notification channel implemented inside `NotificationCore`, while external channels like Telegram remain separate transport adapters/microservices.

Important assumption for this rollout:
- remove old `WebPush`
- replace it with `InApp`
- no backward-compatibility migration is needed because there are no users yet

## Current Implementation Status

### Done

- `WebPush` renamed to `InApp` in frontend and backend enums/contracts
- basic inbox endpoint added: `GET /api/notifications/me`
- dashboard shows latest notifications after login

### In progress now

- create SignalR hub for realtime in-app delivery
- wire backend notification creation flow to push live updates
- connect frontend authenticated shell to the hub
- update notification list and unread count caches on incoming messages

---

## Final Architecture Decision

### Core ownership

`NotificationCore` becomes the owner of:
- notification persistence
- notification preferences
- unread/read state
- notification inbox APIs
- realtime in-app delivery through SignalR

### Separate transport services

External channels stay separate transport adapters:
- `TelegramAgent` stays as Telegram delivery service
- future email or mobile push channels can also be implemented as separate consumers

### Why `InApp` belongs in `NotificationCore`

`InApp` is not an external integration. It is part of the application's own notification domain, so it should live where notifications are stored and managed.

This avoids:
- duplicating notification state in another service
- splitting read/unread logic from notification storage
- extra service hops for internal UI delivery

---

## Current State Summary

### Already exists

- Notification persistence in `Services/NotificationCore/NotificationCore.API/Entities/Notification.cs`
- User channel settings in `Services/NotificationCore/NotificationCore.API/Entities/UserChannel.cs`
- Notification creation from domain events in:
  - `Services/NotificationCore/NotificationCore.API/Events/Events/EventCreatedConsumer.cs`
  - `Services/NotificationCore/NotificationCore.API/Events/Events/RoomInspectionStatusUpdatedConsumer.cs`
- Notification settings UI in `frontend/dorm-app/src/components/user-profile/NotificationSettings.tsx`
- Telegram delivery consumer in `Services/TelegramAgent/TelegramAgent.API/Features/NotificationDelivery.cs`

### Missing

- SignalR hub in backend
- realtime client in frontend
- notification inbox API
- unread count API
- mark-as-read API
- proper `InApp` channel name in frontend and backend

---

## Target User Flow

1. User enables `InApp` notifications in settings.
2. Some domain event creates a notification in `NotificationCore`.
3. `NotificationCore` saves notification in database.
4. `NotificationCore` sends the saved notification to the connected user via SignalR.
5. Frontend receives the message and:
   - shows a toast
   - updates unread count
   - updates notification list
6. User opens notification center and marks one or all notifications as read.

If the user is offline, the notification is still available later because the database is the source of truth.

---

## Scope Of Changes

## 1. Backend: NotificationCore

### 1.1 Rename notification channel

Replace `WebPush` with `InApp` in backend enum and contracts.

Primary file:
- `Services/NotificationCore/NotificationCore.API/Entities/Enums.cs`

Related places to align:
- request/response DTOs under notification settings features
- seed data in `Services/NotificationCore/NotificationCore.API/Data/SeedData.cs`
- any frontend contracts using the same channel list

### 1.2 Add notification inbox APIs

Add new Vertical Slice features in `NotificationCore` for:
- `GetMyNotifications`
- `GetUnreadCount`
- `MarkNotificationAsRead`
- `MarkAllNotificationsAsRead`

Expected route direction:
- `GET /api/notifications/me`
- `GET /api/notifications/me/unread-count`
- `PATCH /api/notifications/{id}/read`
- `PATCH /api/notifications/me/read-all`

Notes:
- use authenticated current user from JWT, not arbitrary `userId` route input
- support newest-first ordering
- for first version, pagination can be simple but should be designed in a way that can be expanded later

### 1.3 Add SignalR hub

Add SignalR into `NotificationCore.API`:
- register SignalR in `Program.cs`
- add hub endpoint
- secure hub with JWT authentication
- target messages to a specific authenticated user

Recommended endpoint shape:
- `/api/notifications/hubs/in-app`

### 1.4 Push notifications after save

Current consumers already:
- build notifications
- save them in DB
- publish `NotificationCreatedIntegrationEvent`

Extend this flow so that after saving notifications, `NotificationCore` also sends realtime SignalR messages for users who have `InApp` enabled.

Important rule:
- persistence first
- realtime push second
- external integration event publishing remains in place

### 1.5 Tighten settings API auth

Current settings read endpoint is based on `userId` path and should be normalized.

Recommended direction:
- replace `GET /notifications/settings/{userId}` with `GET /notifications/settings/me`
- keep `PATCH /notifications/settings/me`
- ensure both are authorized

This keeps notification preferences aligned with the authenticated user model.

### 1.6 Clarify preference logic

Use two-level preference logic:
- notification type setting decides whether user should receive this notification category at all
- channel setting decides which delivery paths are allowed

For `InApp`:
- if channel disabled, no realtime SignalR push
- notification can still exist in DB if product decision says inbox is canonical

Recommended product rule for first version:
- if type is enabled, notification is created in DB
- if `InApp` is enabled, it is also pushed live to the client

This gives a consistent inbox while letting users opt out of live in-app delivery.

---

## 2. Backend: Auth and SignalR Security

### 2.1 JWT support for hub connections

Current shared JWT setup is standard bearer authentication.

SignalR usually needs token extraction from query string during WebSocket connection, so NotificationCore will likely need custom `JwtBearerEvents.OnMessageReceived` handling for the hub route.

Goal:
- browser client connects with JWT
- hub resolves authenticated user id from claims
- server sends notifications to the correct user only

### 2.2 User identification strategy

Recommended approach:
- use JWT-authenticated user id as SignalR user identity
- avoid custom connection registration tables in first version unless needed

This keeps the solution simple and consistent with the rest of the system.

---

## 3. API Gateway

### 3.1 Route notification REST APIs

Notification REST traffic already goes through the gateway under `/api/notifications/...`.

Verify and update routing if needed in:
- `Services/ApiGateway/appsettings.json`

### 3.2 Route SignalR hub traffic

Ensure gateway correctly proxies SignalR negotiation and WebSocket traffic for the new hub endpoint.

If needed, add explicit route support for:
- `/api/notifications/hubs/{**catch-all}`

---

## 4. Frontend

### 4.1 Rename channel in UI contracts

Replace `WebPush` with `InApp` in:
- `frontend/dorm-app/src/lib/types/notification.ts`
- `frontend/dorm-app/src/components/user-profile/NotificationSettings.tsx`

Also update labels and implementation-status config so UI reflects the actual architecture.

### 4.2 Add SignalR client

Add `@microsoft/signalr` to frontend.

Create a client-side notifications module responsible for:
- building the SignalR connection
- supplying JWT access token
- reconnecting automatically
- exposing handlers for incoming notifications

Recommended placement:
- `frontend/dorm-app/src/lib/services/` or `frontend/dorm-app/src/lib/realtime/`

### 4.3 Mount connection once for authenticated app

Create and maintain one connection in the authenticated app shell.

Best integration point:
- `frontend/dorm-app/src/routes/_mainLayout.tsx`

Behavior:
- connect only for authenticated users
- disconnect on logout
- reconnect on transient failures

### 4.4 Update UI when notification arrives

On incoming SignalR message:
- show Sonner toast
- invalidate or update React Query cache
- refresh unread count
- refresh or prepend notification list

Reuse existing notification hook/service pattern:
- `frontend/dorm-app/src/lib/hooks/useNotification.ts`
- `frontend/dorm-app/src/lib/services/notificationService.ts`

### 4.5 Add notification center UI

Use the existing bell button as the entry point.

Current location:
- `frontend/dorm-app/src/routes/_mainLayout/index.tsx`

First version should include:
- unread badge on bell icon
- dropdown, sheet, or popover with recent notifications
- mark-one-as-read
- mark-all-as-read

Optional later enhancements:
- deep links to related pages
- grouped notifications
- filter by type
- dedicated full-page inbox

---

## 5. Delivery Model

### Canonical model

Database is canonical.

SignalR is delivery only.

That means:
- every created notification is stored
- SignalR improves immediacy for online users
- unread/read state is consistent across refreshes and devices

### Channel model

- `InApp` = internal realtime UI channel in `NotificationCore`
- `Telegram` = external transport handled by `TelegramAgent`
- future `Email` = likely separate adapter

---

## 6. Implementation Order

### Phase 1: Rename and cleanup

1. Remove old `WebPush` from frontend and backend.
2. Replace it with `InApp` in enums, types, and settings UI.
3. Update seed/default notification channel configuration.

### Phase 2: Normalize notification settings API

1. Add `GET /notifications/settings/me`.
2. Keep `PATCH /notifications/settings/me`.
3. Ensure both endpoints require authentication.

### Phase 3: Add inbox APIs

1. Add list notifications endpoint.
2. Add unread count endpoint.
3. Add mark-as-read endpoint.
4. Add mark-all-as-read endpoint.

### Phase 4: Add SignalR backend

1. Register SignalR.
2. Add hub.
3. Configure JWT token extraction for hub path.
4. Send notifications to connected users.

### Phase 5: Add frontend realtime client

1. Install SignalR package.
2. Create connection manager.
3. Connect in authenticated layout.
4. Handle incoming messages.

### Phase 6: Add notification center UI

1. Show unread badge.
2. Show recent notifications list.
3. Support read actions.
4. Keep toasts for immediate visibility.

### Phase 7: Regression validation

1. Verify Telegram delivery still works.
2. Verify new in-app flow works end-to-end.
3. Verify auth and reconnect behavior.

---

## 7. Risks And Mitigations

### Risk: SignalR auth issues

Problem:
- JWT setup currently targets standard bearer API requests

Mitigation:
- explicitly handle token extraction for hub route
- test both initial connect and reconnect

### Risk: Settings logic remains inconsistent across channels

Problem:
- current external delivery path does not fully enforce channel settings

Mitigation:
- keep first version focused on `InApp`
- later refactor external delivery paths to consistently respect channel settings

### Risk: UI shows stale unread count

Problem:
- realtime updates and REST cache can diverge

Mitigation:
- use React Query invalidation or deterministic cache update on each incoming message and read action

### Risk: Gateway websocket proxy issues

Problem:
- hub may work locally without gateway but fail through API Gateway

Mitigation:
- validate negotiation and WebSocket upgrade through gateway early

---

## 8. Testing Plan

### Backend tests / verification

- creating event notifications still stores DB rows
- creating inspection notifications still stores DB rows
- `InApp` enabled user receives SignalR message
- `InApp` disabled user does not receive live push
- unread count changes after receive and read actions
- mark-one and mark-all endpoints update DB correctly

### Frontend tests / verification

- authenticated user connects automatically
- toast appears on incoming notification
- bell badge updates correctly
- notification list refreshes correctly
- mark-as-read updates UI without page reload
- logout disconnects realtime client

### End-to-end verification

1. Open app as authenticated user.
2. Enable `InApp` in settings.
3. Trigger a notification-producing action.
4. Verify toast appears.
5. Verify unread badge increments.
6. Verify item appears in notification center.
7. Mark it read and verify unread count decreases.
8. Verify Telegram still works for linked users if channel enabled.

---

## 9. Out Of Scope For First Iteration

- browser service worker push notifications
- mobile push delivery
- advanced delivery audit per channel
- notification grouping and batching
- user-customizable quiet hours
- cross-service generic notification templating engine

---

## 10. Recommended First Build Slice

Build the smallest complete vertical slice in this order:

1. rename `WebPush` to `InApp`
2. add `GET /notifications/me`
3. add `GET /notifications/me/unread-count`
4. add `PATCH /notifications/{id}/read`
5. add SignalR hub
6. connect frontend and show toast
7. add unread badge on bell
8. add notification dropdown or sheet

This delivers visible value quickly while keeping the architecture clean.

---

## Final Recommendation

Implement `InApp` notifications inside `NotificationCore` with:
- persistent DB-backed notifications
- authenticated SignalR live delivery
- frontend bell badge and notification center
- `TelegramAgent` left as separate external transport adapter

This is the simplest architecture that matches the current codebase and scales cleanly for future channels.
