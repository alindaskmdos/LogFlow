<p align="center">
  <h1 align="center">LogFlow</h1>
  <p align="center">
    Централизованный сбор, хранение и аналитика логов
    <br />
    ASP.NET Core · ClickHouse · Redis · Serilog
  </p>
</p>

<p align="center">
  <a href="#быстрый-старт">Быстрый старт</a> ·
  <a href="#sdk">SDK</a> ·
  <a href="#api">API</a>
</p>

---

## О проекте

LogFlow — self-hosted система для сбора структурированных логов из .NET-приложений. Принимает логи по HTTP, буферизирует через in-memory channel и batch-вставками записывает в ClickHouse.

| Компонент | Описание |
|---|---|
| **LogFlow.Api** | HTTP API для приёма логов и аналитических запросов |
| **LogFlow.Sdk** | .NET SDK — HTTP-клиент + Serilog sink ([NuGet](https://www.nuget.org/packages/LogFlow.Sdk)) |
| **LogFlow.DemoApi** | Демо-приложение с примером интеграции SDK |

## Возможности

**Ingestion** — Batch-приём логов, bounded channel очередь, фоновый воркер для batch-вставок, авторизация по API-ключам (SHA-256), rate limiting (1000 запросов / 10 сек), валидация через FluentValidation.

**Аналитика** — Запрос логов по временному диапазону и уровню, граф активности с настраиваемым интервалом, топ самых частых ошибок.

**SDK** — Serilog sink с периодическим батчингом, настройка размера батча и интервала, фильтрация только HTTP-запросов, поддержка .NET 8 и .NET 10.

**Инфраструктура** — ClickHouse с партиционированием по месяцам и TTL 14 дней, Redis-кэш API-ключей, Seq для внутренней диагностики, health checks, Docker Compose.

## Быстрый старт

### Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) и Docker Compose

### Запуск

```bash
cd LogFlow.Api
cp .env.example .env   # при необходимости поменять пароли
docker compose up -d
```

После запуска:

| Сервис | URL |
|---|---|
| LogFlow API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Seq | http://localhost:5341 |
| ClickHouse | http://localhost:8123 |
| Health Check | http://localhost:5000/health |

### Тестовые API-ключи

Для локальной разработки предустановлены три ключа:

| Ключ | Сервис | Активен |
|---|---|---|
| `logflow-test-1` | DemoService | ✅ |
| `logflow-test-2` | DemoService | ❌ |
| `logflow-test-3` | NotDemoService | ✅ |

Передаётся через заголовок `x-api-key`.

---

## SDK

### Установка

```bash
dotnet add package LogFlow.Sdk
```

### Serilog Sink

SDK предоставляет Serilog sink с автоматическим батчингом.

```csharp
using LogFlow.Sdk.Sinks;

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Logger(sub => sub
            .WriteTo.LogFlow(options =>
            {
                options.Url = "http://localhost:5000";
                options.ApiKey = "logflow-test-1";
                options.BatchSize = 100;
                options.Period = TimeSpan.FromSeconds(10);
                options.IncludeOnlyRequestLogs = true;
            }));
});
```

При включённом `IncludeOnlyRequestLogs` пересылаются только логи от `Serilog.AspNetCore.RequestLoggingMiddleware` — удобно для сбора метрик HTTP-запросов без лишнего шума.

### Конфигурация

| Параметр | Тип | По умолчанию | Описание |
|---|---|---|---|
| `Url` | `string` | `""` | Базовый URL LogFlow API |
| `ApiKey` | `string` | `""` | API-ключ |
| `BatchSize` | `int` | `100` | Макс. логов в батче |
| `Period` | `TimeSpan` | `10s` | Интервал отправки |
| `IncludeOnlyRequestLogs` | `bool` | `false` | Только логи HTTP-запросов |

---

## API

Все эндпоинты требуют заголовок `x-api-key`. Имя сервиса определяется автоматически по ключу.

### Приём логов

```
POST /log/LogIngestion
```

Принимает JSON-массив логов (макс. 1000 за запрос). Rate limit: 1000 запросов / 10 сек.

```json
[
  {
    "timestamp": "2026-06-07T12:00:00Z",
    "environment": "Production",
    "level": "Information",
    "message": "GET /api/users completed",
    "traceId": "abc123",
    "requestPath": "/api/users",
    "method": "GET",
    "statusCode": "200",
    "elapsedMs": "12",
    "properties": "{\"userId\": 42}"
  }
]
```

Ответ: `202 Accepted` или `503` если внутренняя очередь переполнена.

### Получение логов

```
GET /api/statistics/logs
```

| Параметр | Тип | Обязателен | По умолчанию | Описание |
|---|---|---|---|---|
| `from` | `DateTimeOffset` | ✅ | — | Начало диапазона |
| `to` | `DateTimeOffset` | ✅ | — | Конец диапазона |
| `level` | `string` | ❌ | — | Фильтр по уровню |
| `limit` | `int` | ❌ | `100` | Макс. записей (1–1000) |

### Частые ошибки

```
GET /api/statistics/errors/frequent
```

Возвращает ошибки, сгруппированные по сообщению, с количеством и временем последнего появления.

| Параметр | Тип | Обязателен | По умолчанию |
|---|---|---|---|
| `from` | `DateTimeOffset` | ✅ | — |
| `to` | `DateTimeOffset` | ✅ | — |
| `limit` | `int` | ❌ | `10` (1–100) |

### Граф активности

```
GET /api/statistics/activity
```

Возвращает количество логов, агрегированных по временным интервалам.

| Параметр | Тип | Обязателен | Описание |
|---|---|---|---|
| `from` | `DateTimeOffset` | ✅ | Начало диапазона |
| `to` | `DateTimeOffset` | ✅ | Конец диапазона |
| `interval` | `TimeSpan` | ✅ | Интервал агрегации (1 мин – 1 день) |
| `level` | `string` | ❌ | Фильтр по уровню |

### Health Check

```
GET /health
```

Проверяет состояние ClickHouse, Redis и Seq.

---

## Схема данных

Таблица `logflow.logs` — MergeTree, партиционирование по месяцам, сортировка по `(Service, Level, Timestamp)`, TTL 14 дней.

| Поле | Тип | Описание |
|---|---|---|
| `Timestamp` | `DateTime64(3)` | Время лога (UTC) |
| `Service` | `LowCardinality(String)` | Имя сервиса (из API-ключа) |
| `Environment` | `LowCardinality(String)` | Окружение |
| `Level` | `LowCardinality(String)` | Уровень лога |
| `Message` | `String` | Сообщение |
| `Exception` | `Nullable(String)` | Исключение |
| `TraceId` / `SpanId` | `Nullable(String)` | Distributed tracing |
| `RequestPath` | `Nullable(String)` | Путь запроса |
| `Method` | `Nullable(String)` | HTTP-метод |
| `StatusCode` | `Nullable(String)` | HTTP-статус |
| `ElapsedMs` | `Nullable(String)` | Длительность запроса |
| `Properties` | `Nullable(String)` | Произвольные свойства (JSON) |

## Стек

| Слой | Технология |
|---|---|
| API | ASP.NET Core (.NET 10) |
| Хранилище | ClickHouse |
| Кэш | Redis |
| Валидация | FluentValidation |
| Логирование | Serilog + Seq |
| Контейнеризация | Docker Compose |