# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DormSystem is a microservices-based dormitory management system featuring user authentication, event management, room booking, file storage, inspections, notifications, and Telegram bot integration. The system uses a React frontend with a .NET 8.0 backend consisting of multiple independent microservices.

## Technology Stack

**Frontend:**
- React 19 with TypeScript
- Vite as build tool
- TanStack Router for routing
- TanStack Query for data fetching
- shadcn/ui component library
- Tailwind CSS for styling
- **Package manager: npm** (NOT yarn, despite what older docs say)

**Backend:**
- .NET 8.0 / .NET 10.0
- PostgreSQL as primary database (all services)
- RabbitMQ (MassTransit) for message bus
- EF Core for data access

**Infrastructure:**
- **.NET Aspire for orchestration** (recommended - manages all services and infrastructure)
- Docker Compose (fallback option for infrastructure only)
- YARP API Gateway with service discovery
- OpenTelemetry for observability

## Architecture Patterns

The backend services use **two different architectural patterns**:

### 1. Three-Layer Architecture (Auth Service)
- **API Layer**: ASP.NET Core controllers
- **BLL (Business Logic Layer)**: Service classes, business logic
- **DAL (Data Access Layer)**: EF Core DbContext, repositories, entities

### 2. Vertical Slice Architecture (Events, Rooms, and newer services)
- **Contracts/**: DTOs and request/response models
- **Entities/**: Database models
- **Features/**: Each feature is self-contained with:
  - `Command` or `Query` (MediatR request)
  - `Handler` (IRequestHandler)
  - `Validator` (FluentValidation)
  - `Endpoint` (Carter endpoint registration)
- **Database/**: EF Core DbContext
- **Services/**: Cross-cutting services
- **Extensions/**: DI and middleware configuration

**Key libraries for Vertical Slice:**
- **Carter**: Endpoint routing and registration
- **MediatR**: CQRS pattern implementation
- **FluentValidation**: Request validation
- **Mapster**: Object-to-object mapping
- **ErrorOr**: Functional error handling
- **MassTransit**: Message bus integration

## Microservices

The system includes the following services:

- **Auth**: User authentication, JWT tokens, role management (3-layer architecture)
- **Events**: Event creation, management, invitations (Vertical Slice)
- **Rooms**: Room management, photos, reservations (Vertical Slice)
- **Booking**: Laundry and facility booking
- **FileStorage**: File upload and management
- **Inspections**: Room inspections
- **NotificationCore**: Notification system
- **TelegramAgent**: Telegram bot integration

**Shared Libraries** (`Services/Shared/`):
- `ServiceDefaults`: Aspire integration (OpenTelemetry, health checks, service discovery)
- `Shared.TokenService`: JWT authentication/authorization helpers
- `Shared.Data`: Common data models and contracts
- `Shared.PagedList`: Pagination utilities
- `Shared.FileServiceClient`: File service client with service discovery
- `Shared.RoomServiceClient`: Room service client with service discovery
- `Shared.UserServiceClient`: User service client with service discovery

## Common Development Commands

### Frontend

```bash
# Navigate to frontend
cd frontend/dorm-app

# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Lint code
npm run lint

# Format code
npm run format

# Check formatting
npm run format:check
```

### Backend

```bash
# Run a specific service
cd Services/Auth/Auth.API
dotnet run

# Run from solution root
cd Services/Auth
dotnet run --project Auth.API/Auth.API.csproj

# Build a service
dotnet build

# Restore dependencies
dotnet restore

# Run EF Core migrations
dotnet ef database update

# Create a new migration
dotnet ef migrations add MigrationName
```

## Running the Application

### Option 1: .NET Aspire (Recommended)

Start all services and infrastructure with unified dashboard:

```bash
cd Services/AspireOrchestration
dotnet run --project AspireOrchestration.AppHost
```

**Aspire Dashboard**: Automatically opens in browser (typically http://localhost:15001)
- **View all services, databases, message queues in one place**
- **Monitor logs, traces, metrics** via OpenTelemetry
- **Health status** for all components
- **Auto-assigned ports** - check dashboard for actual endpoints

**Infrastructure managed by Aspire:**
- PostgreSQL with 6 databases (auth-db, events-db, rooms-db, inspections-db, notifications-db, telegram-db)
- pgAdmin for database management
- RabbitMQ with management UI
- All 9 microservices + API Gateway

**Benefits:**
- ✅ One command starts everything
- ✅ No hardcoded URLs - automatic service discovery
- ✅ Built-in observability (OpenTelemetry)
- ✅ Production-ready configuration

### Option 2: Docker Compose (Fallback)

Start infrastructure only, then run services manually:

```bash
# Start infrastructure
docker-compose -f docker-compose.local.yml up -d

# Run services individually via IDE or dotnet CLI
cd Services/Auth/Auth.API
dotnet run
```

**Infrastructure Services:**
- PostgreSQL: `localhost:5432` (user: postgres, pass: postgres)
- pgAdmin: `localhost:5050` (email: admin@admin.com, pass: admin)
- RabbitMQ Management: `localhost:15672` (user: guest, pass: guest)
- RabbitMQ AMQP: `localhost:5672`

### Testing

```bash
# Run tests for a specific service
cd Services/Events
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Service Communication and Discovery

Services communicate via:
1. **Synchronous HTTP calls** through the API Gateway or directly via service clients (Shared.*ServiceClient)
2. **Asynchronous messaging** via RabbitMQ/MassTransit for events like `EventCreated`

**Service Discovery (Aspire Mode):**
- Services use Aspire service names: `https+http://auth-service`, `https+http://event-service`, etc.
- No hardcoded URLs - automatic resolution via Microsoft.Extensions.ServiceDiscovery
- API Gateway uses YARP service discovery resolver
- Service clients auto-detect Aspire mode and use service discovery

**Fallback Mode (Docker Compose):**
- Services read URLs from appsettings.json (e.g., `http://localhost:5001`)
- Developers manually start services on specific ports
- Connection strings and RabbitMQ settings configured in appsettings

**Auto-Detection:**
All services automatically detect which mode they're running in:
- **Database**: If connection string contains `Host=`, uses PostgreSQL; otherwise SQLite
- **RabbitMQ**: If `rabbitmq` connection string exists, uses it; otherwise reads from RabbitMq config section
- **Service Clients**: If no URL provided, uses Aspire service discovery; otherwise uses provided URL

Example from Events service:
```csharp
await _bus.Publish(new EventCreated
{
    EventId = newEvent.Id,
    Name = newEvent.Name,
    OwnerId = newEvent.OwnerId,
}, cancellationToken);
```

## Important Coding Guidelines

### Frontend
- **Never use `any` type** - use specific types or interfaces
- **No inline styles** - use Tailwind CSS classes
- Use functional components and hooks, not class components
- Use shadcn/ui for UI components: https://ui.shadcn.com/docs/components
- Use TanStack Query for data fetching and caching
- Use TanStack Router for routing
- Use react-hook-form with zod for form validation
- Store all configuration in `.env` files (API URLs, feature flags)

### Backend (.NET)

**Vertical Slice Pattern (for new features in Events, Rooms, etc.):**

Each feature lives in a single file under `Features/` directory:

```csharp
// Features/Events/CreateEvent.cs
public static class CreateEvent
{
    // 1. Command/Query
    internal sealed class Command : IRequest<ErrorOr<Guid>>
    {
        public string Name { get; set; }
        // ... other properties
    }

    // 2. Validator
    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    // 3. Handler
    internal sealed class Handler : IRequestHandler<Command, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Business logic here
        }
    }
}

// 4. Endpoint (Carter)
public sealed class CreateEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/events", async (...) => { })
            .RequireAuthorization();
    }
}
```

**General Backend Rules:**
- Use `ErrorOr<T>` for error handling instead of exceptions
- Use Mapster for object mapping
- Always validate inputs with FluentValidation
- Use structured logging with ILogger
- Authorization: Use `ITokenService.GetUserId(HttpContext)` to get current user
- Database: All services use PostgreSQL in Aspire mode, SQLite in Docker Compose fallback mode (auto-detected)

### Error Handling Pattern

```csharp
var result = await mediator.Send(command);

return result.Match(
    success => Results.Ok(success),
    error => error.ToResponse()  // Extension method from ErrorMappingExtensions
);
```

## Database Configuration

**Aspire Mode (Recommended):**
All services use PostgreSQL with separate databases:
- **Auth**: `auth-db`
- **Events**: `events-db`
- **Rooms**: `rooms-db`
- **Inspections**: `inspections-db`
- **NotificationCore**: `notifications-db`
- **TelegramAgent**: `telegram-db`

Databases are automatically created and managed by Aspire. Migrations run automatically on startup.

**Docker Compose Fallback Mode:**
Services use SQLite files (events_dev.db, rooms.db, etc.) for simpler local development.

**Auto-Detection:**
Services detect database type from connection string - if it contains `Host=`, PostgreSQL is used; otherwise SQLite.

## Authentication Flow

1. User authenticates via Auth service (`/api/auth/login` or `/api/auth/register`)
2. Auth service returns JWT access token and refresh token
3. Frontend stores tokens and includes access token in `Authorization: Bearer {token}` header
4. Services validate JWT using `Shared.TokenService`
5. Use refresh token endpoint to get new access token when expired

**Token Service Usage in Endpoints:**
```csharp
var userIdResult = tokenService.GetUserId(httpContext);
if (userIdResult.IsError)
{
    return Results.Unauthorized();
}
var userId = userIdResult.Value;
```

## File References

When referencing code locations, use the format:
- `Services/Events/Events.API/Features/Events/CreateEvent.cs:100`
- `frontend/dorm-app/src/routes/events.tsx:45`

## API Gateway

The YARP API Gateway (`Services/ApiGateway/`) routes requests to microservices. Frontend should call gateway URLs, not individual services directly (in production).

## Message Bus Events

Services publish domain events via MassTransit/RabbitMQ. Key events:
- `EventCreated` (from Events service)
- Check `Shared.Data` for common event contracts

## Solution Structure

- `Services/DormSystem.slnx`: Root solution containing all services
- `Services/{ServiceName}/{ServiceName}.slnx`: Individual service solutions
- `Services/Shared/`: Cross-cutting shared libraries
- `frontend/dorm-app/`: React SPA

## Configuration Management

**Frontend**: Use `.env` files with `VITE_` prefix:
```
VITE_AUTH_API_URL=http://localhost:5001
VITE_EVENTS_API_URL=http://localhost:5002
```

**Backend**: Use `appsettings.json` and `appsettings.Development.json`:
- Connection strings
- JWT settings
- Service URLs
- RabbitMQ configuration

## Key Architectural Decisions

1. **Microservices over monolith** for scalability and independent deployment
2. **Vertical Slice for new services** (easier to reason about, better cohesion)
3. **Three-layer for Auth** (legacy, but stable)
4. **Message bus for async operations** (event notifications, cross-service communication)
5. **Shared libraries for common concerns** (JWT, data contracts, service clients)
6. **SQLite for development** (easier setup), **PostgreSQL for production** (scalability)

## Development Workflow Best Practices

- Only make changes explicitly requested - avoid over-engineering
- When adding features to Vertical Slice services, follow the single-file pattern
- Use ErrorOr for error handling, not try-catch for business logic
- Always validate inputs with FluentValidation
- Keep frontend config in `.env`, never hardcode URLs
- When creating new endpoints, always use Carter module pattern
- Update PROJECT_DOCUMENTATION.md when making significant architectural changes
