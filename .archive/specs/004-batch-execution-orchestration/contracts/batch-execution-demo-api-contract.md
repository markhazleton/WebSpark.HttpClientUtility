# Batch Execution Demo API Contract

**Application**: WebSpark.HttpClientUtility.Web  
**Feature Area**: BatchExecution demo  
**Goal**: Allow the MVC demo page to start a capped batch run and poll for live statistics until completion.

---

## Endpoints

### `GET /BatchExecution`

Returns the Razor page that hosts the interactive batch execution demo.

**Response**:
- `200 OK` with the pre-populated form and client-side script for starting demo runs and polling status.

---

### `POST /BatchExecution/runs`

Starts a capped demo batch run.

**Request body**:

```json
{
  "environments": [
    { "name": "Local", "baseUrl": "https://localhost:5001" },
    { "name": "Staging", "baseUrl": "https://staging.example.com" }
  ],
  "users": [
    {
      "userId": "john.doe",
      "properties": {
        "userId": "42",
        "firstName": "John",
        "lastName": "Doe"
      }
    }
  ],
  "requests": [
    {
      "name": "GetProfile",
      "method": "GET",
      "pathTemplate": "/api/users/{userId}",
      "bodyTemplate": null
    }
  ],
  "iterations": 2,
  "maxConcurrency": 4
}
```

**Rules**:
- The controller must reject payloads whose planned request count exceeds the demo cap.
- The controller must normalize missing optional collections to empty collections.
- The controller must start the run asynchronously and retain in-memory progress for polling.

**Responses**:

`202 Accepted`

```json
{
  "runId": "batch-20260317-001",
  "totalPlannedCount": 8,
  "status": "Queued"
}
```

`400 Bad Request`

```json
{
  "error": "The demo is limited to 50 planned requests per run."
}
```

---

### `GET /BatchExecution/runs/{runId}`

Returns the current run state plus the latest statistics snapshot. When the run is complete, the full final result is included.

**Response while running**:

```json
{
  "runId": "batch-20260317-001",
  "status": "Running",
  "completedCount": 3,
  "totalPlannedCount": 8,
  "statistics": {
    "totalCount": 3,
    "successCount": 3,
    "failureCount": 0,
    "p50Milliseconds": 112,
    "p95Milliseconds": 143,
    "p99Milliseconds": 143,
    "byEnvironment": {
      "Local": 2,
      "Staging": 1
    }
  }
}
```

**Response when complete**:

```json
{
  "runId": "batch-20260317-001",
  "status": "Completed",
  "completedCount": 8,
  "totalPlannedCount": 8,
  "statistics": {
    "totalCount": 8,
    "successCount": 7,
    "failureCount": 1,
    "p50Milliseconds": 120,
    "p95Milliseconds": 180,
    "p99Milliseconds": 180,
    "byEnvironment": {
      "Local": 4,
      "Staging": 4
    },
    "byMethod": {
      "GET": 8
    },
    "byStatusCode": {
      "200": 7,
      "500": 1
    }
  },
  "results": [
    {
      "environmentName": "Local",
      "userId": "john.doe",
      "requestName": "GetProfile",
      "httpMethod": "GET",
      "requestPath": "https://localhost:5001/api/users/42",
      "isSuccess": true,
      "statusCode": 200,
      "responseBodyHash": "9f0b...",
      "durationMilliseconds": 110,
      "timestampUtc": "2026-03-17T15:00:00Z"
    }
  ]
}
```

`404 Not Found`

```json
{
  "error": "Run not found or expired."
}
```

---

## UI Behavior Contract

- The page submits configuration to `POST /BatchExecution/runs`.
- On `202 Accepted`, the page stores `runId` and starts polling `GET /BatchExecution/runs/{runId}`.
- The page updates stat cards and breakdown tables on each polling response.
- When the run reaches `Completed`, `Cancelled`, or `Failed`, polling stops and the results grid is rendered.