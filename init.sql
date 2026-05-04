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
ORDER BY (Service, Level, Timestamp) TTL Date + toIntervalDay(14) SETTINGS index_granularity = 8192