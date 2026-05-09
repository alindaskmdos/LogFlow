CREATE DATABASE IF NOT EXISTS logflow;

CREATE TABLE IF NOT EXISTS logflow.logs
(
    `Timestamp` DateTime64(3, 'UTC'),
    `Service` LowCardinality(String),
    `Environment` LowCardinality(String),
    `Level` LowCardinality(String),
    `Message` String,
    `Exception` Nullable(String),
    `TraceId` Nullable(String),
    `SpanId` Nullable(String),
    `RequestPath` Nullable(String),
    `Method` Nullable(String),
    `StatusCode` Nullable(Int32),
    `ElapsedMs` Nullable(Int64),
    `Properties` Nullable(String)
)
ENGINE = MergeTree
PARTITION BY toYYYYMM(toDate(Timestamp))
ORDER BY (Service, Level, Timestamp)
TTL toDate(Timestamp) + toIntervalDay(14)
SETTINGS index_granularity = 8192;

CREATE TABLE IF NOT EXISTS logflow.api_keys
(
    ApiKeyHash String,
    ServiceName String,
    IsActive Bool DEFAULT true
)
ENGINE = MergeTree()
ORDER BY ApiKeyHash;

INSERT INTO logflow.api_keys
(
    ApiKeyHash,
    ServiceName,
    IsActive
)
VALUES
('284173775c67105964d6134e9335974e2abe689175e0dfa7a183cb81a4548c83', 'DemoService', true),
('c631f4497c0b9790f614db6a9fe506938e4d26b55de869ecab39486677479919', 'DemoService', false),
('0d5ef03e188ec93b359dc9c8043c9120a9db06dfe536fdd15626c81bdb43fb8b', 'NotDemoService', true);

/*
logflow-test-1
logflow-test-2
logflow-test-3
*/