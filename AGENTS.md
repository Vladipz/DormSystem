# AGENTS.md

This file is the primary repo guide for coding agents working in DormSystem. Human-facing project onboarding now lives in `README.md`.

## Repo Summary

DormSystem is a microservices-based dormitory management platform with a React frontend and a .NET backend. The repository includes independent services for auth, events, rooms, booking, file storage, inspections, notifications, Telegram integration, a YARP API gateway, and shared service libraries.

## Source Of Truth

- Use `README.md` for project overview, setup, and common development commands.
- Use this file for repo-specific implementation rules and architectural constraints.
- Do not reintroduce duplicate top-level docs for the same purpose.

## Repository Layout

- `frontend/dorm-app` contains the React application.
- `Services/` contains all backend services, shared libraries, and Aspire orchestration.
- `Services/DormSystem.slnx` is the main .NET solution entry point.
- `Services/Shared/` contains shared infrastructure and cross-service libraries.

## Architecture Rules

Two backend architecture styles exist in the repo. Match the existing service pattern instead of mixing them.

### Auth Service

`Auth` uses a three-layer structure:

- API layer for controllers and HTTP entry points
- BLL layer for business logic and services
- DAL layer for EF Core, repositories, and entities

### Events, Rooms, and Newer Services

Newer services use Vertical Slice architecture with feature-oriented organization:

- `Contracts/` for DTOs and request models
- `Entities/` for persistence models
- `Features/` for command/query, handler, validator, and endpoint logic
- `Database/` for DbContext and persistence setup
- `Services/` for cross-cutting service classes
- `Extensions/` for DI and middleware registration

For Vertical Slice services:

- keep new features aligned with existing feature boundaries
- use MediatR for commands and queries
- validate requests with FluentValidation
- expose endpoints with Carter
- use Mapster for mapping where the service already relies on it
- return `ErrorOr<T>` instead of using exceptions for business-flow control

## Frontend Rules

- Use TypeScript without `any`.
- Use functional React components and hooks.
- Use Tailwind CSS classes instead of inline styles.
- Follow existing shadcn/ui usage patterns.
- Use TanStack Router for routing and TanStack Query for server state.
- Prefer `react-hook-form` with `zod` for forms.
- Keep configuration in `.env` files with `VITE_` variables.

## Backend Rules

- Use `ITokenService.GetUserId(HttpContext)` for current-user access.
- Use structured logging with `ILogger`.
- Validate inputs with FluentValidation.
- Prefer `ErrorOr<T>` and response mapping helpers for endpoint results.
- Keep service URLs and secrets in configuration, not hardcoded in source.

Endpoint result pattern:

```csharp
var result = await mediator.Send(command);

return result.Match(
    success => Results.Ok(success),
    error => error.ToResponse()
);
```

## Runtime And Infrastructure Notes

- Aspire is the recommended way to run the system locally.
- Docker Compose is the fallback path.
- Services communicate through HTTP and RabbitMQ/MassTransit.
- Under Aspire, services should rely on service discovery instead of hardcoded local addresses.
- Database mode is auto-detected from the connection string configuration.

## Commands Agents Commonly Need

Frontend:

```bash
cd frontend/dorm-app
npm install
npm run dev
npm run build
npm run lint
npm run typecheck
```

Backend:

```bash
cd Services
dotnet restore DormSystem.slnx
dotnet build DormSystem.slnx
```

Run Aspire:

```bash
cd Services/AspireOrchestration
dotnet run --project AspireOrchestration.AppHost
```

Run tests:

```bash
cd Services/Events
dotnet test
```

## Change Discipline

- Make only the changes required by the task.
- Follow the established architecture of the service you are editing.
- Avoid moving code between architectural layers unless the task requires it.
- Do not introduce new documentation files when `README.md` or `AGENTS.md` should be updated instead.
- Update human-facing docs when changing setup, architecture, or workflows.
- Update this file only when agent behavior or repo-specific implementation rules change.

## File Reference Format

When pointing to code locations, use repository-relative paths like:

- `Services/Events/Events.API/Features/Events/CreateEvent.cs:100`
- `frontend/dorm-app/src/routes/events.tsx:45`
