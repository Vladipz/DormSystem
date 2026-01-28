# Таски та плани DormSystem

Ця папка містить плани міграцій, технічні завдання та звіти про виконану роботу.

## Структура

```
.tasks/
├── README.md                          # Цей файл
├── phase-1-completed.md              # ✅ Звіт про Фазу 1 (Централізація пакетів)
└── migration-phase-2-dotnet10.md     # 📋 План Фази 2 (Міграція на .NET 10)
```

---

## Поточний статус міграції

### ✅ Фаза 1: Централізація управління пакетами (.NET 8)
**Статус:** ЗАВЕРШЕНО
**Дата:** 2026-01-28
**Файл:** `phase-1-completed.md`

**Досягнення:**
- Створено централізоване керування NuGet пакетами
- Видалено 12 локальних конфігураційних файлів
- Оновлено 17 .csproj проектів
- Вирішено критичні проблеми з Shared.TokenService
- Стандартизовано версії пакетів (MassTransit 8.4.0, тощо)
- Білд успішний: 0 помилок

---

### 🔄 Фаза 2: Оновлення до .NET 10
**Статус:** ГОТОВО ДО ВИКОНАННЯ
**Файл:** `migration-phase-2-dotnet10.md`

**Що буде зроблено:**
1. Оновити Directory.Packages.props (версії пакетів .NET 10)
2. Оновити Directory.Build.props (net8.0 → net10.0)
3. Перевірити EF Core міграції
4. Валідація (restore, build, tests)
5. End-to-End мануальне тестування

**Очікуваний час:** 2-3 години

---

## Як використовувати плани

### Переглянути поточний план
```bash
cd /home/vlad/data/Code/Projects/DormSystem/.tasks
cat migration-phase-2-dotnet10.md
```

### Переглянути завершені роботи
```bash
cat phase-1-completed.md
```

### Виконати Фазу 2
```bash
# 1. Відкрити план
less migration-phase-2-dotnet10.md

# 2. Слідувати крокам:
# - Крок 2.1: Оновити Directory.Packages.props
# - Крок 2.2: Оновити Directory.Build.props
# - Крок 2.3: Перевірити міграції
# - Крок 2.4: Валідація
# - Крок 2.5: Тестування

# 3. Після завершення створити звіт:
# cp phase-1-completed.md phase-2-completed.md
# # Відредагувати phase-2-completed.md
```

---

## Корисні команди

### Швидка перевірка статусу
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services

# Поточна версія .NET
grep "TargetFramework" Directory.Build.props

# Кількість централізованих пакетів
grep -c "PackageVersion Include" Directory.Packages.props

# Білд
dotnet build DormSystem.sln
```

### Перевірка пакетів
```bash
cd /home/vlad/data/Code/Projects/DormSystem/Services

# Вразливі пакети
dotnet list package --vulnerable

# Застарілі пакети
dotnet list package --deprecated

# Outdated пакети
dotnet list package --outdated
```

---

## Історія міграцій

| Дата | Фаза | Статус | Результат |
|------|------|--------|-----------|
| 2026-01-28 | Фаза 1: CPM (.NET 8) | ✅ Завершено | 0 помилок, 17 проектів оновлено |
| TBD | Фаза 2: .NET 10 | 📋 Запланована | - |

---

## Контакти та питання

Якщо виникають питання під час виконання планів:
1. Перечитати розділ "Ризики та мітігація" у відповідному плані
2. Перевірити "План відкату" для повернення до попереднього стану
3. Звернутися до "Корисні ресурси" для документації Microsoft

---

**Останнє оновлення:** 2026-01-28
