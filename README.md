# DormSystem

DormSystem is a dormitory management platform built as a microservices-based system. It combines a React frontend with a .NET backend to support core dorm workflows such as authentication, events, room management, bookings, inspections, notifications, file storage, and Telegram integration.

## Product Scope

The platform is intended to centralize everyday dorm operations for residents and administrators. The current repository and project vision cover:

- authentication and resident account flows
- event creation and participation
- room and facility management
- laundry and booking scenarios
- inspections and operational tracking
- notifications across multiple channels
- file uploads and storage
- Telegram-based integration points

## What Is In This Repository

- `frontend/dorm-app` contains the React 19 + TypeScript + Vite web client.
- `Services/` contains backend microservices, shared libraries, the API gateway, and Aspire orchestration.
- `docker/` and `docker-compose.yml` provide the fallback local infrastructure setup.
- `AGENTS.md` contains repo-specific instructions for coding agents and automated contributors.

## Core Services

- `Auth` handles authentication, JWT tokens, and role-related flows.
- `Events` handles event creation, invitations, and participant flows.
- `Rooms` manages rooms, room photos, and reservations.
- `Booking` supports laundry and facility booking scenarios.
- `FileStorage` handles file uploads and storage access.
- `Inspections` manages room inspections.
- `NotificationCore` provides notification workflows.
- `TelegramAgent` integrates the system with Telegram.
- `ApiGateway` routes external traffic to the internal services.

Shared libraries live under `Services/Shared/` and cover common concerns such as token validation, service defaults, and inter-service clients.

## Architecture Overview

DormSystem uses two backend architecture styles:

- `Auth` follows a classic three-layer design with API, BLL, and DAL projects.
- `Events`, `Rooms`, and newer services follow a Vertical Slice pattern built around feature-oriented handlers, validators, and Carter endpoints.

The platform also relies on:

- PostgreSQL for service data
- RabbitMQ and MassTransit for asynchronous messaging
- YARP for API gateway routing
- .NET Aspire for orchestration, service discovery, and observability

## Technology Stack

### Frontend

- React 19
- TypeScript
- Vite
- TanStack Router
- TanStack Query
- shadcn/ui
- Tailwind CSS
- npm as the package manager

### Backend

- .NET 8.0 / .NET 10.0
- ASP.NET Core
- EF Core
- Carter
- MediatR
- FluentValidation
- Mapster
- ErrorOr
- MassTransit

### Infrastructure

- .NET Aspire
- Docker Compose
- PostgreSQL
- RabbitMQ
- OpenTelemetry

## Getting Started

### Prerequisites

- Node.js and npm
- .NET SDK
- Docker Desktop or Docker Engine for local containers

### Recommended Local Run: Aspire

Aspire is the preferred way to run the whole system because it starts the application services and infrastructure together.

```bash
cd Services/AspireOrchestration
dotnet run --project AspireOrchestration.AppHost
```

Use the Aspire dashboard to inspect service health, logs, traces, and the actual service endpoints assigned for the current run.

### Fallback Local Run: Docker Compose + Manual Services

Use Docker Compose when you want local infrastructure without the full Aspire workflow.

```bash
docker compose up -d --build
```

This starts infrastructure and containerized services defined in `docker-compose.yml`. If you want to run specific services manually during development, start the needed infrastructure first and then run the corresponding .NET project locally.

Useful local endpoints:

- frontend: `http://localhost:3001`
- API gateway: `http://localhost:5095`
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`

### Stress Test VM: Vagrant + Prebuilt Images

For a disposable stress-test environment, the repo includes:

- `Vagrantfile` for a VM with 4 vCPU and 4 GB RAM
- `vagrant/bootstrap.sh` to install Docker Engine and Docker Compose inside the guest
- `docker-compose.vm.yml` to run the stack from prebuilt container images instead of building from source

Recommended host setup:

- run Vagrant from Windows or another non-WSL host path
- if your repo currently lives only inside WSL, copy or clone it to a Windows-accessible directory first, for example `C:\vm\DormSystem`
- create `.env` from `.env.example` before starting the VM

Bring up the VM from the repository root:

```bash
vagrant up
vagrant ssh
```

Inside the VM, start the stack:

```bash
dormsystem-up
```

Useful endpoints from the host machine:

- frontend: `http://localhost:3001`
- API gateway: `http://localhost:5095`
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`
- RabbitMQ management: `http://localhost:15672`

The frontend image in `docker-compose.vm.yml` is a prebuilt static Vite bundle. Its `VITE_API_GATEWAY_URL` value is baked in during image build, so changing container environment variables at startup will not change which API URL the browser uses.

If 4 GB RAM is too tight for the full observability stack, start only the core services first:

```bash
cd /workspace
docker compose -f docker-compose.vm.yml up -d postgres rabbitmq file-storage-service auth-service event-service room-service inspection-service notification-service booking-service api-gateway frontend
```

## Common Commands

### Frontend

```bash
cd frontend/dorm-app
npm install
npm run dev
npm run build
npm run lint
npm run typecheck
npm run format
npm run format:check
```

### Backend

```bash
cd Services
dotnet restore DormSystem.slnx
dotnet build DormSystem.slnx
```

Run a specific service:

```bash
cd Services/Auth
dotnet run --project Auth.API/Auth.API.csproj
```

Run tests for a service:

```bash
cd Services/Events
dotnet test
```

## Configuration

### Frontend

Frontend environment variables live in `.env` files and should use the `VITE_` prefix.

The frontend currently uses:

```env
VITE_API_GATEWAY_URL=http://localhost:5095
VITE_NOTIFICATIONS_DELIVERY_MODE=websocket
```

When running the frontend from Docker, these values are resolved at image build time, not at container start time. Choose the URL that the user's browser will use to reach the API gateway.

- Browser-visible public URL example: `http://localhost:5095`
- Docker-internal service URL example: `http://api-gateway:8080`

Use the browser-visible URL for `VITE_API_GATEWAY_URL`. Docker-internal hostnames are only valid for container-to-container traffic, not for JavaScript running in the browser.

### Backend

Backend configuration lives in `appsettings.json` and `appsettings.Development.json`.

Typical configuration includes:

- connection strings
- JWT settings
- service URLs
- RabbitMQ settings

## Service Communication

Services communicate in two ways:

- synchronous HTTP calls through the API Gateway or shared service clients
- asynchronous domain events through RabbitMQ and MassTransit

When running under Aspire, services use service discovery instead of hardcoded local URLs.

## Documentation Map

- Start here for project overview, setup, and day-to-day development commands.
- Read `AGENTS.md` for repository rules aimed at coding agents and automated contributors.

Legacy project documents have been reduced to short redirect notes so this file remains the primary human-facing source of truth.
