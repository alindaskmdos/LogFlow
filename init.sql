CREATE DATABASE IF NOT EXISTS logflow;

CREATE TABLE logflow.logs 
    ( `Timestamp` DateTime64(3, 'UTC'), `Date` Date DEFAULT toDate(Timestamp), 
    `Service` LowCardinality(String), 
    `Environment` LowCardinality(String), 
    `Level` LowCardinality(String), 
    `Message` String, 
    `Exception` String, 
    `TraceId` String, 
    `SpanId` String, 
    `RequestPath` String, 
    `Method` LowCardinality(String), 
    `StatusCode` UInt16, 
    `ElapsedMs` UInt32, 
    `Properties` String ) 
ENGINE = MergeTree PARTITION BY toYYYYMM(Date) 
ORDER BY (Service, Level, Timestamp) TTL Date + toIntervalDay(14) SETTINGS index_granularity = 8192;

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
('230ca3470ff45ff2b412bf7af3eacc678dc71f654b1f485db777265ba9ca6329', 'DemoService', true),
('e3fc4efacdb9f0eb018e69e71af7fcac03b0d2a842b10959f6d4d12eb0fccb48', 'DemoService', false),
('7bc83f4384efbac67d02fc9b9a3dc5bb7c041fed38287ffed78788ea18fbfa4a', 'NotDemoService', true);