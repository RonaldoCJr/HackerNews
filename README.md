# Hacker News API - Technical Challenge

A high-performance REST API built with **.NET 10 (ASP.NET Core)** that consumes the official
Hacker News Firebase API and exposes the top *n* best stories ordered by score descending.

This project was developed as a technical challenge, with a focus on **clean architecture**,
**scalability** and **safe consumption of the upstream API**.

---

## Assumptions

* The upstream Hacker News API has no documented rate limit, but it should still be
  protected — caching is therefore mandatory and not optional.
* The list of best stories changes frequently; story details are effectively immutable
  (only score and comment count drift slowly). Different TTLs are applied accordingly.
* `n` may be larger than the number of available best stories; in that case, the API
  returns all stories it could retrieve, ordered by score.
* The challenge requires a single endpoint, so authentication, rate-limiting and
  multi-tenant concerns are intentionally out of scope.

---

## Performance & Scalability

* **Parallel fetching:** Story details are fetched concurrently via `Task.WhenAll`, drastically
  reducing the overall response time when retrieving large values of *n*.
* **Caching abstraction (`ICacheService`):** A dedicated abstraction centralizes all cache
  access, so the implementation can be swapped (in-memory, Redis, hybrid) without touching
  the application layer. The current implementation uses `IMemoryCache`:
    * `GET /v0/beststories.json` (the list of best story IDs) is cached for **20 seconds**.
      The list changes frequently on Hacker News, so a short TTL keeps results fresh while
      still absorbing bursts of incoming requests against a single upstream call.
    * `GET /v0/item/{id}.json` (each individual story) is cached for **20 minutes**
      (configurable via `CacheSettings:DefaultExpirationMinutes`). Story content is
      essentially immutable — only score and comment count drift slowly — so a longer TTL
      protects the upstream API and yields near-zero latency for repeated reads.

This means that under heavy load, a single upstream call to Hacker News can serve thousands
of incoming requests, and the API never overloads the upstream service.

---

## Resilience & Fault Tolerance

The Refit HTTP client is wrapped in a multi-layered resilience pipeline configured via 
`Policy.WrapAsync`. The execution order flows from the outer layer (Circuit Breaker) down to 
the closest layer to the network (Timeout):

* **Timeout Policy (2s):** Enforces a strict 2-second limit per HTTP request. If the upstream 
  API hangs, it triggers a `TimeoutRejectedException`.
* **Retry Policy with Exponential Backoff (3 Attempts):** If a transient error occurs 
  (5xx or Timeout), the API retries the request up to **3 times**. The delay increases exponentially
  based on $2^{\text{attempt}}$ (2s, 4s, and 8s), allowing the upstream service room to recover.
* **Circuit Breaker Policy (5 failures / 15s):** Breaks the circuit if **5 consecutive failures**
  occur, entering the *Open* state for **15 seconds** to shield our system and stop hammering the 
  failing upstream service.

⚠️ **Scope Implication (Crucial Distinction):**
* **Retries are Per-User / Per-Request:** The 3 retry attempts are isolated to the single scope 
  of that specific incoming user request.
* **Circuit Breaker is Global:** The 5 consecutive failures are tracked **globally across all 
  concurrent users** hitting the API. If the global threshold is violated, the circuit opens for 
  *everyone*,   immediately short-circuiting failing calls to the `ExceptionMiddleware` without 
  wasting threads or network sockets.

---

## Versioning & Compatibility

The API follows a versioning strategy via URL segments (e.g., /v1/story/...) to ensure stability. 
Our compatibility policy is defined as follows:
* **Backward Compatibility**: We guarantee that existing fields will not be removed or altered in 
  a way that breaks current clients.
* **Forward Compatibility**: Clients are expected to gracefully handle and ignore any additional 
  fields that may be introduced in future updates.

---

## Architecture

The project follows the principles of **Clean Architecture**, adapted into a single-project
solution to balance simplicity for the technical challenge with a strict separation of
concerns through namespaces and folders:

```text
HackerNews/
├── appsettings.json              # Application configuration (key/value)
├── HackerNews.csproj             # Project file with dependencies (Refit, Swagger, etc.)
├── Program.cs                    # Composition Root: DI and middleware pipeline
│
├── API/                          # Presentation Layer
│   └── Controllers/
│       └── v1/                   # Versioned API endpoints
│           └── StoryController.cs
│
├── Application/                  # Application Logic Layer (Orchestration)
│   ├── DTOs/                     # Data Transfer Objects for API contracts
│   ├── Interfaces/               # Application and repository contracts (abstractions)
│   ├── Mappers/                  # Object-to-Object mapping (Entities → DTOs)
│   └── Services/                 # Application services implementing business use cases
│
├── Domain/                       # Enterprise Domain Layer
│   └── Entities/                 # Pure business POCO entities (abstract Item, Story)
│
└── Infrastructure/               # External Concerns Layer
    ├── Cache/                    # Caching implementation (IMemoryCache today; Redis-ready)
    ├── Exceptions/               # Custom technical exceptions (external service errors)
    ├── Extensions/               # Extension methods for infrastructure settings, bootstrapping and DI
    ├── Interfaces/               # Internal infrastructure contracts (Refit API definition)
    ├── Middleware/               # Global exception handling and logging
    ├── Repositories/             # Data access implementation (Repository pattern)
    ├── Resilience/               # Resilience and fault tolerance for external I/Os
    └── Settings/                 # Strongly typed configuration models
```

## Design Patterns & Principles

* **SOLID Principles:** High level of decoupling and interface-based programming.
* **Repository Pattern:** Abstracts the data source (Hacker News Firebase API).
* **DTO Pattern:** Keeps the internal domain isolated and the public contract stable.
* **Inversion of Control (IoC):** All dependencies are injected via the native .NET DI
  container.
* **Global Exception Handling:** Centralized middleware for logging and translating technical and business
  errors into consistent HTTP responses.

---

## Technology Stack

* **Framework:** .NET 10 (ASP.NET Core)
* **HTTP Client:** [Refit](https://github.com/reactiveui/refit) — type-safe REST client
* **Caching:** `Microsoft.Extensions.Caching.Memory` (`IMemoryCache`)
* **API Versioning:** `Asp.Versioning.Mvc` (URL Segment strategy)
* **Documentation:** Swashbuckle (Swagger / OpenAPI) with versioning support

### NuGet Packages

| Package | Version | Purpose |
| --- | --- | --- |
| `Asp.Versioning.Mvc` | 10.0.0 | API versioning support for MVC |
| `Asp.Versioning.Mvc.ApiExplorer` | 10.0.0 | API Explorer for versioned endpoints |
| `Microsoft.AspNetCore.OpenApi` | 10.0.3 | Built-in OpenAPI support |
| `Refit.HttpClientFactory` | 10.1.6 | Type-safe HTTP client for the upstream API |
| `Swashbuckle.AspNetCore` | 10.1.7 | Swagger / OpenAPI documentation UI |
| `Microsoft.Extensions.Http.Polly` | 10.0.8 | Seamless integration between HttpClient factory and Polly policies |
| `Polly` | 8.6.6 | Core resilience and transient-fault-handling library |

---

## Possible Enhancements

* **Distributed L2 cache (Redis):** If traffic grows, adding a Redis-based L2 cache on top
  of the in-memory L1 would allow horizontal scaling of the API while keeping the upstream
  load constant across all instances.
* **Background refresh / cache warm-up:** A hosted service could periodically refresh the
  best-stories list and pre-fetch story details, so user requests are always served from
  hot cache (cache-aside → refresh-ahead pattern)
* **Dockerization:** Provide a `Dockerfile` and `docker-compose` setup for containerized 
  deployment and environment consistency.
* **Pagination & limits:** Enforce a maximum *n* and add pagination headers to protect both
  the API and the upstream service from accidental abuse.
* **Observability:** Structured logging, OpenTelemetry traces and metrics (cache hit ratio,
  upstream latency, request rate) for production-grade visibility.

---

## How to Run

### Prerequisites

* Install the **.NET 10 SDK**.

### Configuration

Check `appsettings.json` for the following sections:

* `HackerNewsApi` → `BaseUrl` and `Version` of the upstream API.
* `CacheSettings` → `DefaultExpirationMinutes` for the in-memory cache.

### Execution

```bash
dotnet restore
dotnet run
```

### Swagger UI

Once the application is running, navigate to:

```
https://localhost:5071/swagger/index.html
```

> The exact port may differ depending on your environment — check the console output when
> the application starts.

---

## API Endpoints

### `GET /v1/story/bests/limit={n}`

Fetches the top `n` best stories from Hacker News, ordered by **score** in descending order.

**Path parameter**

| Name | Type | Description |
| --- | --- | --- |
| `n` | `int` | Number of stories to retrieve. Must be greater than zero. |

**Sample response (Success - HTTP 200 OK)**

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

**Sample response (Error - HTTP 4xx/5xx ProblemDetails)**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231",
  "title": "Service Temporarily Unavailable (Circuit Breaker)",
  "status": 502,
  "detail": "Upstream service is temporarily unavailable. Please try again later.",
  "instance": "/v1/story/bests/limit=10"
}
```

**Response codes**

| Code | Description |
| --- | --- |
| `200 OK` | Returns the list of best stories. |
| `400 Bad Request` | `n` is less than or equal to zero. |
| `500 Internal Server Error` | Unhandled internal server error. |
| `502 Bad Gateway` | Error communicating with the upstream Hacker News API or Circuit Breaker triggered. |
| `504 Gateway Timeout` | The request to the upstream Hacker News API timed out via resilience policy. |


---

**Developed by:** Ronaldo Carrasco Junior
