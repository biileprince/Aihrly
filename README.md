# Aihrly API

ASP.NET Core 9 Web API for an Applicant Tracking System (ATS). Covers the team-side hiring pipeline: posting jobs, managing applications, scoring candidates, tracking stage changes, and logging notes.

## Tech Stack

- **.NET 9** ASP.NET Core Web API
- **PostgreSQL 16** via EF Core + Npgsql
- **Redis 7** for response caching (`GET /api/applications/{id}`)
- **Swashbuckle** for Swagger UI (`/swagger`)
- **xUnit** + `WebApplicationFactory` for tests

---

## Running Locally

### With Docker (recommended)

Requires Docker Desktop running.

```bash
# Start PostgreSQL and Redis
docker compose up -d

# Apply migrations and run the API
dotnet ef database update --project Aihrly.Api.csproj
dotnet run --project Aihrly.Api.csproj
```

The API will be available at `http://localhost:5067`. Swagger UI is accessible at `http://localhost:5067/swagger`.

### Without Docker

You need a local PostgreSQL instance and Redis. Update `appsettings.Development.json` with your connection details, then:

```bash
dotnet ef database update --project Aihrly.Api.csproj
dotnet run --project Aihrly.Api.csproj
```

Default connection strings (matching docker-compose):

| Service    | Value                                                                           |
| ---------- | ------------------------------------------------------------------------------- |
| PostgreSQL | `Host=localhost;Port=55432;Database=aihrly;Username=postgres;Password=postgres` |
| Redis      | `localhost:6380`                                                                |

---

## Running the Tests

```bash
dotnet test Aihrly.Api.Tests/Aihrly.Api.Tests.csproj
```

Tests use an in-memory database and `DistributedMemoryCache` — no external services required.

---

## Seeded Team Members

Three team members are inserted by the initial migration and are available immediately on a fresh database:

| ID | Name         | Email                      | Role          |
| -- | ------------ | -------------------------- | ------------- |
| 1  | Alex Johnson | `alex.johnson@aihrly.test` | Recruiter     |
| 2  | Sam Patel    | `sam.patel@aihrly.test`    | HiringManager |
| 3  | Jordan Lee   | `jordan.lee@aihrly.test`   | Recruiter     |

Pass one of these IDs in the `X-Team-Member-Id` header on any mutating request.

---

## API Overview

All mutating endpoints (except `POST /api/jobs/{jobId}/applications`) require the `X-Team-Member-Id` header. Missing or invalid values return `401` with an `application/problem+json` body.

### Jobs

| Method | Route           | Description                                            |
| ------ | --------------- | ------------------------------------------------------ |
| POST   | `/api/jobs`     | Create a job                                           |
| GET    | `/api/jobs`     | List jobs (`?status=open`, `?page=1&pageSize=20`)      |
| GET    | `/api/jobs/{id}`| Get a single job                                       |

### Applications

| Method | Route                            | Description                                      |
| ------ | -------------------------------- | ------------------------------------------------ |
| POST   | `/api/jobs/{jobId}/applications` | Candidate applies (public — no header needed)    |
| GET    | `/api/jobs/{jobId}/applications` | List applications for a job (`?stage=screening`) |
| GET    | `/api/applications/{id}`         | Full candidate profile (cached 60 s in Redis)    |
| PATCH  | `/api/applications/{id}/stage`   | Move to a new stage                              |

### Notes

| Method | Route                          | Description                |
| ------ | ------------------------------ | -------------------------- |
| POST   | `/api/applications/{id}/notes` | Add a note                 |
| GET    | `/api/applications/{id}/notes` | List notes, newest first   |

### Scores

| Method | Route                                       | Description             |
| ------ | ------------------------------------------- | ----------------------- |
| PUT    | `/api/applications/{id}/scores/culture-fit` | Set culture-fit score   |
| PUT    | `/api/applications/{id}/scores/interview`   | Set interview score     |
| PUT    | `/api/applications/{id}/scores/assessment`  | Set assessment score    |

### Pipeline Stages and Valid Transitions

```text
Applied → Screening → Interview → Offer → Hired
Applied / Screening / Interview / Offer → Rejected
```

Invalid transitions return `400` with a descriptive error message.

---

## Part 2 — Deep Dive: Redis Caching (Option B)

`GET /api/applications/{id}` — the full candidate profile endpoint — is cached in Redis with a **60-second TTL**. The cache key is `application:{id}`.

The cache is invalidated on every write that touches the application:

- Stage change (`PATCH /api/applications/{id}/stage`)
- Note added (`POST /api/applications/{id}/notes`)
- Any score updated (`PUT /api/applications/{id}/scores/*`)

**When it helps:** The profile endpoint is the most read-heavy call in the pipeline UI. Under concurrent team review sessions (multiple recruiters looking at the same candidate at the same time), caching avoids repeated joined queries across five tables.

**When it could hurt:** With a 60-second TTL, a recruiter who just added a note or moved a stage might see stale data on a hard refresh if the invalidation path failed silently (e.g. Redis down). The invalidation calls are best-effort — they do not fail the write if Redis is unavailable. Acceptable for a take-home; in production you'd want at least a log and alerting on cache errors.

---

## Assumptions

- **Score storage is denormalized onto the `Applications` row.** Three sets of `(Score, Comment, UpdatedById, UpdatedAt)` columns live directly on the table. This keeps the profile query to a single row fetch plus joined collections, at the cost of not retaining score history. The assessment prompt says "PUT semantics — submitting again overwrites", so this is intentional.
- **Stage enum values are stored as integers** in the database. Enum names are accepted as strings in API request bodies via the default JSON serialization.
- **The public apply endpoint does not require a team member header.** Only `POST /api/jobs/{jobId}/applications` is truly public. All others require `X-Team-Member-Id`.
- **Rejected and Hired are terminal stages.** Once an application reaches either, no further transitions are allowed.
- **Email uniqueness is enforced per job**, not globally. The same candidate can apply to multiple different jobs.

---

## What I'd Improve With More Time

- **Score history:** Add a `ScoreHistory` table so the product team can audit every score change, not just the current value.
- **Cursor-based pagination:** The current `?page=N&pageSize=N` approach works but can skip or duplicate rows under concurrent inserts. Keyset/cursor pagination is safer for high-throughput pipelines.
- **Structured logging:** Switch to Serilog with a JSON formatter so logs are queryable in a log aggregator.
- **Health check endpoint:** A `/health` route that checks database and Redis connectivity, useful for container orchestration readiness probes.
