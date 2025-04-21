# Документація DormSystem

## Зміст
1. [Огляд проєкту](#%D0%BE%D0%B3%D0%BB%D1%8F%D0%B4-%D0%BF%D1%80%D0%BE%D1%94%D0%BA%D1%82%D1%83)
2. [Технологічний стек](#%D1%82%D0%B5%D1%85%D0%BD%D0%BE%D0%BB%D0%BE%D0%B3%D1%96%D1%87%D0%BD%D0%B8%D0%B9-%D1%81%D1%82%D0%B5%D0%BA)
3. [Архітектура](#%D0%B0%D1%80%D1%85%D1%96%D1%82%D0%B5%D0%BA%D1%82%D1%83%D1%80%D0%B0)
4. [Структура директорій](#%D1%81%D1%82%D1%80%D1%83%D0%BA%D1%82%D1%83%D1%80%D0%B0-%D0%B4%D0%B8%D1%80%D0%B5%D0%BA%D1%82%D0%BE%D1%80%D1%96%D0%B9)
   - [Корінь проєкту](#%D0%BA%D0%BE%D1%80%D1%96%D0%BD%D1%8C-%D0%BF%D1%80%D0%BE%D1%94%D0%BA%D1%82%D1%83)
   - [Frontend](#frontend)
   - [Infrastructure-services](#infrastructure-services)
   - [Services](#services)
   - [Shared](#shared)
5. [Frontend: dorm-app](#frontend-dorm-app)
   - [src/](#src)
   - [Структура сторінок та роутінг](#%D1%81%D1%82%D1%80%D1%83%D0%BA%D1%82%D1%83%D1%80%D0%B0-%D1%81%D1%82%D0%BE%D1%80%D1%96%D0%BD%D0%BE%D0%BA-%D1%82%D0%B0-%D1%80%D0%BE%D1%83%D1%82%D1%96%D0%BD%D0%B3)
6. [Backend-сервіси](#backend-%D1%81%D0%B5%D1%80%D0%B2%D1%96%D1%81%D0%B8)
   - [Auth Service](#auth-service)
   - [Events Service](#events-service)
7. [Інфраструктура](#%D1%96%D0%BD%D1%84%D1%80%D0%B0%D1%81%D1%82%D1%80%D1%83%D0%BA%D1%82%D1%83%D1%80%D0%B0)
8. [Конфігурація та змінні оточення](#%D0%BA%D0%BE%D0%BD%D1%84%D1%96%D0%B3%D1%83%D1%80%D0%B0%D1%86%D1%96%D1%8F-%D1%82%D0%B0-%D0%B7%D0%BC%D1%96%D0%BD%D0%BD%D1%96-%D0%BE%D1%82%D0%BE%D1%87%D0%B5%D0%BD%D0%BD%D1%8F)
9. [Робочий процес](#%D1%80%D0%BE%D0%B1%D0%BE%D1%87%D0%B8%D0%B9-%D0%BF%D1%80%D0%BE%D1%86%D0%B5%D1%81)
10. [Запуск та збірка](#%D0%B7%D0%B0%D0%BF%D1%83%D1%81%D0%BA-%D1%82%D0%B0-%D0%B7%D0%B1%D1%96%D1%80%D0%BA%D0%B0)
11. [Додаткові ресурси](#%D0%B4%D0%BE%D0%B4%D0%B0%D1%82%D0%BA%D0%BE%D0%B2%D1%96-%D1%80%D0%B5%D1%81%D1%83%D1%80%D1%81%D0%B8)

---

## Огляд проєкту
DormSystem — система для управління гуртожитками: автентифікація користувачів, керування подіями, налаштування тощо.

## Технологічний стек
- **Frontend**: React, TypeScript, Vite, TailwindCSS, shadcn/ui, TanStack Query, TanStack Router
- **Backend**: 
  - .NET 8.0
  - **Auth Service**: 
    - Microsoft.AspNetCore.Authentication.JwtBearer
    - ErrorOr
    - Swashbuckle.AspNetCore (Swagger)
    - EF Core
  - **Events Service**: 
    - Carter (endpoint routing)
    - MediatR (CQRS pattern)
    - FluentValidation
    - Mapster (об'єктне мапування)
    - ErrorOr
    - EF Core with Npgsql
- **Інфраструктура**: API Gateway (YARP), Service Discovery, Aspire Orchestration
- **База даних**: PostgreSQL

## Архітектура
Мікросервіси:
- Auth Service (трьохрівнева архітектура: API, BLL, DAL)
- Events Service (Vertical Slice архітектура):
  - Contracts: DTO та інтерфейси
  - Entities: моделі даних
  - Features: команди/запити, обробники, валідатори та ендпоінти
  - Extensions: розширення для DI та конфігурації
  - Database: контекст бази даних

Інфраструктура:
- API Gateway (YARP)
- Service Discovery
- Aspire Orchestration (.NET 8.0)

## Структура директорій

### Корінь проєкту
```plaintext
Docker-compose файли
frontend/               # UI-додаток
Infrastructure-services/
Services/               # Auth, Events та інші мікросервіси
Shared/                 # Загальні бібліотеки та сервіси
```

### Frontend
```plaintext
frontend/dorm-app/
 ├─ public/             # статичні ресурси
 ├─ src/
  │   ├─ assets/
  │   ├─ components/    # UI-компоненти (shadcn/ui + Tailwind)
  │   ├─ hooks/         # React hooks
  │   ├─ lib/           # налаштування клієнтів (react-query, axios)
  │   ├─ routes/        # конфігурація роутів
  │   ├─ App.tsx
  │   └─ main.tsx
  └─ vite.config.ts
```

### Infrastructure-services
```plaintext
Infrastructure-services/
  └─ ApiGateways/ApiGateway.YARP/   # конфіг API Gateway
```

### Services
```plaintext
Services/
  ├─ Auth/      # Auth.sln з проектами API, BLL, DAL
  └─ Events/    # Events.sln з Vertical Slice архітектурою
```

### Shared
```plaintext
Shared/Shared.TokenService/         # бібліотека для JWT авто- та аутентифікації
```

## Frontend: dorm-app

### src/
- **assets/** — зображення, іконки
- **components/** — повторно використовувані UI елементи (shadcn/ui + Tailwind)
- **hooks/** — кастомні React hooks (useAuth, useEvents тощо)
- **lib/** — налаштування клієнтів: axios instance, react-query client
- **routes/** — опис маршрутів (TanStack Router)

### Структура сторінок та роутінг
- `/login` — сторінка входу
- `/register` — реєстрація користувача
- `/dashboard` — огляд (загальна інформація)
- `/events` — список подій
- `/events/:id` — деталі події
- `/events/new` — створення події
- `/profile` — профіль користувача

## Backend-сервіси

### Auth Service
**Архітектура**: 3 шари
- API: контролери ASP.NET Core
- BLL: бізнес-логіка, сервіси Auth, Token
- DAL: EF Core, репозиторії, PostgreSQL

### Events Service
**Архітектура**: Vertical Slice
- Contracts: DTO та інтерфейси
- Features: кожна фіча містить Command/Query, Handler, Validator, Endpoint (з використанням Carter)
- Entities: моделі даних
- Database: контекст EF Core, підключення до PostgreSQL
- Extensions: налаштування DI та middleware

## Інфраструктура
- **API Gateway**: YARP, проксування запитів до мікросервісів
- **Service Discovery**: налаштування відкриття сервісів в середовищі
- **Aspire Orchestration**: управління життєвим циклом мікросервісів

## Конфігурація та змінні оточення
- Усі ключі та URL мікросервісів в `.env` (frontend) та `appsettings*.json` (backend)
- Приклад:
  ```plaintext
  VITE_AUTH_API_URL=http://localhost:5001
  VITE_EVENTS_API_URL=http://localhost:5002
  ```

## Робочий процес
- **Гілки**: `main` (стабільна), `develop` (набір фіч), feature/* для нових задач
- **Код рев’ю**: pull request + автоматичні тести
- **CI/CD**: GitHub Actions для build & deploy

## Запуск та збірка
- **Frontend**:
  ```bash
  cd frontend/dorm-app
  npm install
  npm run dev
  ```
- **Backend**:
  ```plaintext
  cd Services/Auth/Auth.API
  dotnet run
  cd Services/Events/Events.API
  dotnet run
  ```
- **Docker Compose**:
  ```bash
  docker-compose -f docker-compose.local.yml up --build
  ```

## Додаткові ресурси
- shadcn/ui: https://ui.shadcn.com
- TailwindCSS: https://tailwindcss.com/docs
- TanStack Query: https://tanstack.com/query/latest
- TanStack Router: https://tanstack.com/router/latest
- Carter .NET: https://github.com/CarterCommunity/Carter
- ErrorOr: https://github.com/amantinband/error-or
- FluentValidation: https://fluentvalidation.net/
- MediatR: https://github.com/jbogard/MediatR
- Mapster: https://github.com/MapsterMapper/Mapster
