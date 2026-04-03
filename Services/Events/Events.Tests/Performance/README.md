# Performance Test Endpoints

This folder contains `k6` performance tests for the DormSystem APIs.

The safest default target is the API Gateway, because it is the public entry point for the app.

- Default local gateway URL: `http://localhost:5095`
- Current first script: `GetEvents.k6.js`

## Current Implemented Test

- `GET /api/events`

## Available Endpoints For Future k6 Tests

The list below uses gateway routes from `Services/ApiGateway/appsettings.json`.

## Events

- `GET /api/events`
- `GET /api/events/{id}`
- `GET /api/events/{eventId}/participants`
- `GET /api/events/{id}/validate-invitation`
- `GET /api/events/{id}/generate-invitation`
- `POST /api/events`
- `PUT /api/events/{id}`
- `DELETE /api/events/{id}`
- `POST /api/events/{id}/join`
- `POST /api/events/{eventId}/participants`
- `DELETE /api/events/{eventId}/participants/{userId}`

## Auth And Users

- `POST /api/auth/register`
- `POST /api/auth/authorize`
- `POST /api/auth/token`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/protected`
- `GET /api/user`
- `GET /api/user/{userId}`
- `GET /api/user/profile`
- `POST /api/user/{userId}/roles/{roleName}`
- `DELETE /api/user/{userId}/roles/{roleName}`
- `POST /api/user/avatar`
- `DELETE /api/user/avatar`
- `POST /api/link-codes`
- `POST /api/link-codes/validate`

## Buildings

- `GET /api/buildings`
- `GET /api/buildings/{id}`
- `POST /api/buildings`
- `PUT /api/buildings/{id}`
- `DELETE /api/buildings/{id}`

## Floors

- `GET /api/floors`
- `GET /api/floors/{id}`
- `POST /api/floors`
- `PUT /api/floors/{id}`
- `DELETE /api/floors/{id}`

## Blocks

- `GET /api/blocks`
- `GET /api/blocks/{id}`
- `POST /api/blocks`
- `PUT /api/blocks/{id}`
- `DELETE /api/blocks/{id}`

## Rooms

- `GET /api/rooms`
- `GET /api/rooms/{id}`
- `GET /api/rooms/for-inspection`
- `POST /api/rooms`
- `PUT /api/rooms/{id}`
- `DELETE /api/rooms/{id}`
- `POST /api/rooms/{roomId}/photos`
- `DELETE /api/rooms/{roomId}/photos/{photoId}`

## Places

- `GET /api/places`
- `GET /api/places/{id}`
- `GET /api/places/user/{userId}/address`
- `POST /api/places`
- `PUT /api/places/{id}`
- `DELETE /api/places/{id}`
- `PUT /api/places/{id}/occupy`
- `PUT /api/places/{id}/vacate`

## Maintenance Tickets

- `GET /api/maintenance-tickets`
- `GET /api/maintenance-tickets/{id}`
- `POST /api/maintenance-tickets`
- `PUT /api/maintenance-tickets/{id}`
- `PATCH /api/maintenance-tickets/{id}/status`
- `DELETE /api/maintenance-tickets/{id}`

## Inspections

- `GET /api/inspections`
- `GET /api/inspections/{id}`
- `GET /api/inspections/{id}/report`
- `POST /api/inspections`
- `POST /api/inspections/{id}/start`
- `POST /api/inspections/{id}/complete`
- `PATCH /api/inspections/{inspectionId}/rooms/{roomInspectionId}`

## Notifications

- `GET /api/notifications/me`
- `GET /api/notifications/me/changes`
- `POST /api/notifications/test`
- `POST /api/notifications/receipt`
- `PATCH /api/notifications/me/read`
- `GET /api/notifications/settings/me`
- `PATCH /api/notifications/settings/me`
- `GET /api/notifications/hubs/in-app`

## File Storage

- `GET /api/files/{id}`
- `GET /api/files/category/{category}`
- `POST /api/files/upload`
- `DELETE /api/files/{id}`

## Good First Performance Targets

- `GET /api/events`
- `GET /api/events/{id}`
- `GET /api/buildings`
- `GET /api/rooms`
- `GET /api/maintenance-tickets`
- `GET /api/inspections`

## Notes

- Prefer public `GET` endpoints for the first baseline tests.
- Add authenticated tests after the basic smoke and load scenarios are stable.
- File upload and SignalR hub tests are possible, but they are not the best first `k6` scripts.
