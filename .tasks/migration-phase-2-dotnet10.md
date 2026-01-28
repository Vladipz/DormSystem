# ФАЗА 2: Оновлення до .NET 10

## Статус: Готово до виконання
**Фаза 1 завершена успішно** ✅
- Централізоване керування пакетами впроваджено
- Всі проекти використовують .NET 8 з єдиного джерела
- 0 помилок білду

---

## Крок 2.1: Оновити Directory.Packages.props для .NET 10

**Файл:** `/home/vlad/data/Code/Projects/DormSystem/Services/Directory.Packages.props`

**Замінити версії на .NET 10:**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup Label="Core Framework (.NET 10 versions)">
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.2" />
    <PackageVersion Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.2" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.2" />
  </ItemGroup>

  <ItemGroup Label="Entity Framework Core">
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.2" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.2" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup Label="Messaging">
    <PackageVersion Include="MassTransit" Version="8.4.0" />
    <PackageVersion Include="MassTransit.RabbitMQ" Version="8.4.0" />
    <!-- MassTransit v8.4.0 підтримує .NET 10, оновлення на v9 можна зробити пізніше -->
  </ItemGroup>

  <ItemGroup Label="Vertical Slice Architecture">
    <PackageVersion Include="Carter" Version="8.0.0" />
    <PackageVersion Include="MediatR" Version="12.4.1" />
    <PackageVersion Include="Mapster" Version="7.4.0" />
    <PackageVersion Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
    <PackageVersion Include="ErrorOr" Version="2.0.1" />
  </ItemGroup>

  <ItemGroup Label="API Documentation">
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="7.2.0" />
    <PackageVersion Include="Swashbuckle.AspNetCore.Swagger" Version="7.2.0" />
  </ItemGroup>

  <ItemGroup Label="Infrastructure">
    <PackageVersion Include="Yarp.ReverseProxy" Version="2.3.0" />
    <PackageVersion Include="Aspire.Hosting.AppHost" Version="13.1.0" />
  </ItemGroup>

  <ItemGroup Label="Microsoft Extensions">
    <PackageVersion Include="Microsoft.Extensions.Http" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup Label="Service-Specific Libraries">
    <PackageVersion Include="SixLabors.ImageSharp" Version="3.1.8" />
    <PackageVersion Include="Telegram.Bot" Version="22.5.1" />
    <PackageVersion Include="QuestPDF" Version="2024.3.0" />
    <PackageVersion Include="SkiaSharp" Version="3.119.0" />
  </ItemGroup>

  <ItemGroup Label="Code Quality">
    <PackageVersion Include="SonarAnalyzer.CSharp" Version="10.6.0.109712" />
    <PackageVersion Include="StyleCop.Analyzers" Version="1.1.118" />
  </ItemGroup>
</Project>
```

**КЛЮЧОВІ ЗМІНИ:**
- Microsoft.AspNetCore.*: 8.0.12 → 10.0.2
- Microsoft.EntityFrameworkCore.*: 8.0.12 → 10.0.2
- Npgsql.EntityFrameworkCore.PostgreSQL: 8.0.11 → 10.0.0
- Microsoft.Extensions.*: 9.0.4 → 10.0.0
- Aspire.Hosting.AppHost: 9.0.0 → 13.1.0
- MassTransit: залишається 8.4.0 (сумісний з .NET 10)

---

## Крок 2.2: Оновити Directory.Build.props для .NET 10

**Файл:** `/home/vlad/data/Code/Projects/DormSystem/Services/Directory.Build.props`

**Замінити TargetFramework:**

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>  <!-- ЗМІНЕНО з net8.0 -->
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>All</AnalysisMode>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SonarAnalyzer.CSharp" Condition="$(MSBuildProjectExtension) == '.csproj'">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="StyleCop.Analyzers" Condition="$(MSBuildProjectExtension) == '.csproj'">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

**Важливо:** Після цієї зміни ВСІ 18 проектів автоматично перейдуть на `net10.0`.

---

## Крок 2.3: Перевірити EF Core міграції

**Для кожного сервісу з базою даних:**

### Auth Service (PostgreSQL):
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services/Auth/Auth.API

# Спробувати створити тестову міграцію
dotnet ef migrations add VerifyDotNet10 --context AuthDbContext --project ../Auth.DAL/Auth.DAL.csproj

# Якщо міграція пуста (no changes detected) - видалити її
dotnet ef migrations remove
```

### Events Service (SQLite):
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services/Events/Events.API
dotnet ef migrations add VerifyDotNet10 --context EventsDbContext

# Якщо пуста - видалити
dotnet ef migrations remove
```

### Rooms Service (SQLite):
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services/Rooms/Rooms.API
dotnet ef migrations add VerifyDotNet10 --context RoomsDbContext

# Якщо пуста - видалити
dotnet ef migrations remove
```

### Inspections Service:
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services/Inspections/Inspections.API
dotnet ef migrations add VerifyDotNet10 --context InspectionsDbContext

# Якщо пуста - видалити
dotnet ef migrations remove
```

### NotificationCore Service:
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services/NotificationCore/NotificationCore.API
dotnet ef migrations add VerifyDotNet10 --context NotificationDbContext

# Якщо пуста - видалити
dotnet ef migrations remove
```

**Очікуваний результат:**
- ✅ Міграції пусті (no schema changes) - EF Core 10 сумісний
- ⚠️ Якщо є зміни - уважно переглянути перед застосуванням

---

## Крок 2.4: Валідація Фази 2

**Команди для перевірки:**

```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services

# 1. Очистити повністю
dotnet clean DormSystem.sln
rm -rf **/bin **/obj

# 2. Відновити з .NET 10 пакетами
dotnet restore DormSystem.sln

# 3. Білд
dotnet build DormSystem.sln --configuration Debug

# 4. Перевірити що використовується .NET 10
dotnet list package --framework net10.0

# 5. Перевірити outdated (не повинно бути критичних)
dotnet list package --outdated
```

**Критерії успіху Фази 2:**
- ✅ Всі проекти успішно білдяться з `net10.0`
- ✅ EF Core міграції не виявили breaking changes
- ✅ Aspire оновлено до 13.1.0
- ✅ Всі пакети сумісні з .NET 10

---

## Крок 2.5: Мануальне тестування End-to-End

### 1. Запустити інфраструктуру
```bash
cd /home/vlad/data/Code/Projects/DormSystem
docker-compose -f docker-compose.local.yml up -d

# Перевірити що PostgreSQL і RabbitMQ запущені
docker ps
```

### 2. Тестування Auth Service
```bash
cd Services/Auth/Auth.API
dotnet run

# Перевірити endpoints:
# - POST /api/auth/register (реєстрація)
# - POST /api/auth/login (логін, отримання JWT token)
# - POST /api/auth/refresh (оновлення токена)
```

### 3. Тестування Vertical Slice Services

**Events Service:**
```bash
cd Services/Events/Events.API
dotnet run

# Перевірити:
# - Carter endpoints працюють
# - MediatR обробка команд
# - Mapster маппінг
# - FluentValidation валідація
# - MassTransit публікація EventCreated
```

**Rooms Service:**
```bash
cd Services/Rooms/Rooms.API
dotnet run

# Перевірити:
# - Завантаження фото
# - SQLite база працює
# - CRUD операції
```

### 4. Тестування Messaging (MassTransit)
- Створити подію в Events.API
- Перевірити що NotificationCore отримав повідомлення через RabbitMQ
- Переконатись що messages правильно обробляються

### 5. Тестування Infrastructure

**ApiGateway (YARP):**
```bash
cd Services/ApiGateway
dotnet run

# Перевірити reverse proxy:
# - Запити проксуються до Auth, Events, Rooms
# - JWT authentication passthrough працює
# - CORS налаштовано
```

**AspireOrchestration:**
```bash
cd Services/AspireOrchestration/AspireOrchestration.AppHost
dotnet run

# Перевірити:
# - Всі сервіси стартують через Aspire
# - Service discovery працює
# - Logs aggregation
```

### 6. Тестування Service-Specific Features

**FileStorage:**
```bash
cd Services/FileStorage/FileStorage.API
dotnet run

# Перевірити:
# - SixLabors.ImageSharp обробка зображень
# - Завантаження/скачування файлів
```

**Inspections:**
```bash
cd Services/Inspections/Inspections.API
dotnet run

# Перевірити:
# - QuestPDF генерація PDF звітів
# - SkiaSharp графіка
```

**TelegramAgent:**
```bash
cd Services/TelegramAgent/TelegramAgent.API
dotnet run

# Перевірити:
# - Telegram.Bot інтеграція
# - Обробка команд
# - Взаємодія з іншими сервісами
```

### 7. End-to-End Scenario
```
1. Користувач реєструється (Auth) → отримує JWT token ✅
2. Створює подію (Events) → MassTransit публікує EventCreated ✅
3. NotificationCore отримує повідомлення → надсилає нотифікацію ✅
4. Користувач бронює кімнату (Rooms) → завантажує фото ✅
5. Адмін створює інспекцію (Inspections) → генерується PDF ✅
6. Telegram bot отримує команду → запитує сервіси → відповідає ✅
```

---

## Ризики та мітігація

### ВИСОКИЙ РИЗИК:

**1. Aspire 9.0.0 → 13.1.0 (major upgrade)**
- **Ризик:** Breaking changes в orchestration
- **Мітігація:**
  - Переглянути [Aspire 13 release notes](https://aspire.dev/whats-new/aspire-13/)
  - Тестувати orchestration окремо
  - Перевірити що всі сервіси правильно стартують

**2. Carter/MediatR/Mapster сумісність з .NET 10**
- **Ризик:** Vertical Slice архітектура може мати проблеми
- **Мітігація:** Спочатку протестувати Events.API (найскладніший сервіс)

### СЕРЕДНІЙ РИЗИК:

**3. EF Core міграції**
- **Ризик:** Schema changes або несумісність SQL
- **Мітігація:** Backup баз, тестові міграції у Кроці 2.3

### НИЗЬКИЙ РИЗИК:

**4. Service-specific бібліотеки** (Telegram.Bot, QuestPDF, SixLabors.ImageSharp)
- **Ризик:** Несумісність з .NET 10
- **Мітігація:** Зазвичай ці бібліотеки швидко оновлюються під нові .NET

---

## План відкату (Rollback)

**Якщо Фаза 2 не вдалася:**

```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services

# 1. Повернути Directory.Build.props на net8.0
sed -i 's/net10.0/net8.0/g' Directory.Build.props

# 2. Повернути Directory.Packages.props на .NET 8 версії
git checkout Directory.Packages.props

# 3. Очистити та перебілдити
dotnet clean DormSystem.sln
rm -rf **/bin **/obj
dotnet restore DormSystem.sln
dotnet build DormSystem.sln
```

**Альтернативний відкат (повний):**
```bash
# Відновити ВСЕ з git
cd /home/vlad/data/Code/Projects/DormSystem/Services
git checkout Directory.Build.props Directory.Packages.props
dotnet restore DormSystem.sln
dotnet build DormSystem.sln
```

---

## Фінальна верифікація

**Після завершення Фази 2:**

```bash
# 1. Переконатись що всі проекти на net10.0
grep -r "TargetFramework" Services/**/*.csproj
# Повинно бути ТІЛЬКИ в Directory.Build.props

# 2. Перевірити централізоване керування
grep -r "PackageVersion" Services/**/Directory.Packages.props
# Повинно бути ТІЛЬКИ в /Services/Directory.Packages.props

# 3. Білд Release конфігурації
dotnet build DormSystem.sln --configuration Release

# 4. Перевірити розмір артефактів (не повинен істотно збільшитись)
du -sh Services/*/bin/Release/net10.0/

# 5. Performance baseline (опціонально)
# Порівняти startup time та memory usage з .NET 8
```

---

## Документація після міграції

**Оновити файли:**

1. **CLAUDE.md** - додати секцію про .NET 10:
```markdown
## .NET Version
- Backend: .NET 10.0
- SDK: 10.0.101
- Centralized Package Management: Enabled
```

2. **PROJECT_DOCUMENTATION.md** - документувати зміни:
```markdown
## NuGet Package Management
- Використовується Central Package Management (CPM)
- Версії пакетів керуються з `Services/Directory.Packages.props`
- Global properties в `Services/Directory.Build.props`
- Всі проекти на єдиній версії .NET 10.0
```

3. **README.md** - оновити requirements:
```markdown
## Prerequisites
- .NET 10.0 SDK or later
- Docker and Docker Compose
- PostgreSQL (via Docker)
- RabbitMQ (via Docker)
```

---

## CI/CD (якщо є)

**Оновити:**

1. **Docker base images:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

2. **GitHub Actions / Azure Pipelines:**
```yaml
- uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '10.0.x'
```

3. **Перевірити deployment pipelines**

---

## Корисні ресурси

- [Breaking changes in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10)
- [What's New in EF Core 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [Npgsql 10.0 Release Notes](https://www.npgsql.org/efcore/release-notes/10.0.html)
- [What's new in Aspire 13](https://aspire.dev/whats-new/aspire-13/)
- [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)

---

## Наступні кроки після Фази 2

1. **Моніторинг продакшн середовища** (якщо deploy)
2. **Планування міграції на MassTransit v9** (у Q3-Q4 2026)
3. **Встановлення процесу регулярного оновлення пакетів** (щомісяця)
4. **Оновлення onboarding документації**
5. **Вирішення vulnerability в SixLabors.ImageSharp** (оновити до патченої версії)

---

## Підсумок переваг .NET 10

✅ **Performance improvements** - швидше виконання, менше споживання пам'яті
✅ **Нові C# 13 features** - покращений синтаксис, productivity
✅ **ASP.NET Core enhancements** - кращі performance для HTTP endpoints
✅ **EF Core покращення запитів** - оптимізовані SQL queries
✅ **Довша підтримка** - актуальна платформа з активною підтримкою
✅ **Централізоване керування пакетами** - легше оновлювати та підтримувати

---

**Готовність до виконання: ✅**
**Очікуваний час виконання: 2-3 години**
