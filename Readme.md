# LogFlow

LogFlow — lightweight система для централизованного сбора, хранения и анализа логов на базе ASP.NET Core и ClickHouse

Проект состоит из:

* LogFlow.Api — API для ingestion и аналитики логов
* LogFlow.Sdk — SDK для отправки логов из .NET приложений
* ClickHouse — основное хранилище логов
* Redis — кэш API-ключей
* Seq — внутреннее логирование и диагностика системы

Проект находится в стадии активной разработки

---

# Возможности

## Ingestion API

* Прием batch логов через HTTP API
* API-key авторизация
* Асинхронная запись логов
* Очередь ingestion через Channel
* Batch insert в ClickHouse
* Rate limiting
* Health checks

## Аналитика

* Получение логов по временному диапазону
* Фильтрация по уровню логирования
* Граф активности логов
* Анализ наиболее частых ошибок

## SDK

* Отправка логов в LogFlow.Api
* Поддержка Serilog sink
* Поддержка .NET 8 и .NET 10

---

# Архитектура

```text
Application / SDK
        ↓
   LogFlow.Api
        ↓
   Channel Queue
        ↓
  Batch Worker
        ↓
   ClickHouse
```

Логи принимаются API, помещаются в bounded channel, после чего background worker записывает их batch-вставками в ClickHouse

---

# Используемые технологии

| Компонент        | Технология     |
| ---------------- | -------------- |
| Backend API      | ASP.NET Core   |
| Storage          | ClickHouse     |
| Cache            | Redis          |
| Internal Logging | Seq + Serilog  |
| SDK              | .NET SDK       |
| Containerization | Docker Compose |

---

# Структура репозитория

```text
LogFlow/
│
├── LogFlow.Api/
├── LogFlow.Sdk/
├── LogFlow.slnx
└── README.md
```

---

# Быстрый старт

## Требования

* .NET 10 SDK
* Docker
* Docker Compose

---

## Запуск инфраструктуры

Из папки `LogFlow.Api`:

```bash
docker compose up -d
```

После запуска будут доступны:

| Сервис          | URL                                                            |
| --------------- | -------------------------------------------------------------- |
| LogFlow API     | [http://localhost:5000](http://localhost:5000)                 |
| Swagger         | [http://localhost:5000/swagger](http://localhost:5000/swagger) |
| Seq             | [http://localhost:5341](http://localhost:5341)                 |
| ClickHouse HTTP | [http://localhost:8123](http://localhost:8123)                 |

---

# API ключи

Для локального тестирования доступны demo API keys

Передавать ключ необходимо через заголовок:

```http
x-api-key: logflow-test-1
```

Demo ключи предназначены только для локальной разработки и тестирования

---

# Ingestion API

## Endpoint

```http
POST /log/LogIngestion
```

## Пример запроса

```json
[
  {
    "timestamp": "2026-01-01T12:00:00Z",
    "environment": "Development",
    "level": "Information",
    "message": "Request completed",
    "traceId": "trace-123",
    "spanId": "span-123",
    "requestPath": "/api/users",
    "method": "GET",
    "statusCode": "200",
    "elapsedMs": "12",
    "properties": "{\"userId\":1}"
  }
]
```

## Ответ

```http
202 Accepted
```

---

# Statistics API

## Получение логов

```http
GET /api/statistics/logs
```

## Получение графа активности

```http
GET /api/statistics/activity
```

## Получение частых ошибок

```http
GET /api/statistics/errors/frequent
```

---

# Health Checks

Система использует ASP.NET Core Health Checks

Endpoint:

```http
GET /health
```

Проверяются:

* ClickHouse
* Redis
* Seq

---

# SDK

## Установка

```bash
dotnet add package LogFlow.Sdk
```

## Возможности SDK

* HTTP клиент для отправки логов
* Serilog sink
* Контракты запросов и ответов