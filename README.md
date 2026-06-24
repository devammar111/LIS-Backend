# LIS Lab Order API — Backend (.NET 8)

A Laboratory Information System (LIS) service for submitting and tracking lab orders. Built as a
layered ASP.NET Core 8 Web API with SQL Server persistence, JWT authentication with role-based
authorization, input validation, structured logging, an audit trail, pagination/filtering, and
API security controls.

> Companion frontend (Angular 19) lives in the **LIS-Frontend** repository.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (runs the SQL Server container)
- (Frontend only) [Node.js 18/20/22](https://nodejs.org/)

That's it — no local SQL Server install required.

---

## How to run (under 5 minutes)

### 1. Start SQL Server (Docker)

From the repository root:

```bash
docker compose up -d
```

This starts SQL Server 2022 on `localhost:1433` with a named volume so data survives restarts.
Check it is healthy with `docker compose ps`.

### 2. Run the API

```bash
dotnet run --project LIS.Api
```

On startup the API **automatically applies EF Core migrations and seeds users** — with a built-in
retry loop, so it is safe to start even while the SQL container is still warming up. You'll see:

```
Database migrations applied successfully.
Seeded 2 users (admin, tech).
Now listening on: http://localhost:5062
```

- API base URL: `http://localhost:5062`
- Swagger UI (Development): `http://localhost:5062/swagger` — use the **Authorize** button with a
  token from `/api/auth/login` to call protected endpoints.

> Run in Development to enable Swagger and disable HTTPS redirect:
> `dotnet run --project LIS.Api --launch-profile http` (the `http` profile sets
> `ASPNETCORE_ENVIRONMENT=Development`).

### Seeded credentials (dev only)

| Username | Password    | Role       |
|----------|-------------|------------|
| `admin`  | `Admin123!` | Admin      |
| `tech`   | `Tech123!`  | Technician |

### Reset the database

```bash
docker compose down -v   # -v wipes the SQL data volume
```

---

## Persistence choice

**SQL Server 2022 (Docker) + EF Core 8**, code-first migrations.

- **Why:** a lab information system must not lose data on restart, must be queryable, and must
  support audit/reporting — an in-memory store cannot. SQL Server in Docker keeps setup to a single
  `docker compose up` while staying production-representative. Migrations are committed, so the
  schema is reproducible and upgradeable.
- **Tradeoff:** requires Docker locally. The connection string lives in `appsettings.json` for
  developer convenience (see *Configuration & secrets*).
- `DateOnly` is stored via an explicit value converter so ordering/filtering translate to SQL
  server-side identically on both SQL Server and the SQLite provider used in tests.

---

## Architecture

```
Controller  →  Service  →  Repository  →  EF Core / SQL Server
(HTTP only)    (rules)      (data access)
```

- **Controllers** (`OrdersController`, `AuthController`, `AuditController`) — HTTP concerns only:
  model binding, validation invocation, status codes. No business logic.
- **Services** (`OrderService`, `AuthService`, `AuditService`, `JwtTokenService`) — domain rules,
  enum mapping, token issuance, logging, audit of auth events.
- **Repositories** (`OrderRepository`, `UserRepository`) — EF Core data access, server-side paging.
- **Cross-cutting** — FluentValidation validators, a global `IExceptionHandler` (RFC 7807),
  Serilog, a `SaveChanges` audit interceptor, JWT auth, rate limiting, security headers.

All wired through the built-in DI container in `Program.cs`.

---

## API

All `/api/orders` and `/api/audit` endpoints require a `Bearer` token.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | Anonymous (rate-limited 5/min) | Returns a JWT + role + expiry |
| POST | `/api/orders` | Admin or Technician | Create a lab order |
| GET | `/api/orders?page=&pageSize=&priority=` | Admin or Technician | Paged list, newest collection date first |
| GET | `/api/audit?page=&pageSize=` | **Admin only** | Paged audit trail |

**Priority filter:** `?priority=high` (or `STAT`) returns STAT orders only; `Routine` returns
routine only; omitted returns all. The response shape is identical regardless of filter.

**Paged response shape** (consistent for every list endpoint):

```json
{
  "items": [ /* ... */ ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

**Validation failures** return `400` as RFC 7807 `ValidationProblemDetails` (the `errors`
dictionary is keyed by camelCase field name):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "patientName": ["Patient name is required."],
    "testType": ["Test type must be one of: CBC, BMP, Lipid Panel, UA."],
    "priority": ["Priority must be Routine or STAT."],
    "collectionDate": ["Collection date cannot be in the past."]
  }
}
```

Other status codes: `401` (missing/expired token), `403` (role not permitted),
`429` (rate limit exceeded), `500` (unhandled — returned as ProblemDetails, no stack trace).

---

## Security

- **Authentication:** JWT bearer (HS256). Login issues a 60-minute token with `sub`, `unique_name`,
  and `role` claims; 30-second clock skew.
- **Authorization:** a fallback policy requires an authenticated user on every endpoint by default
  (`[AllowAnonymous]` only on login). The audit endpoint is `[Authorize(Roles = "Admin")]`.
- **Passwords:** hashed with ASP.NET Core `PasswordHasher<T>` (PBKDF2) — never stored in plaintext.
- **Rate limiting:** global fixed window (100/min per IP) + a stricter `login` policy (5/min).
- **Headers:** `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; Kestrel `Server`
  header suppressed. CORS restricted to `http://localhost:4200`.

## Logging & audit

- **Serilog** writes structured logs to the console and a rolling file (`logs/lis-.log`, 7-day
  retention). Request logging is enabled; business events (order created, login success/failure)
  are logged explicitly.
- **Audit trail:** an EF Core `SaveChanges` interceptor records every order creation; `AuthService`
  records login success/failure. Each row captures user, action, entity, timestamp, and details —
  viewable via `GET /api/audit` (Admin).

---

## Configuration & secrets

`appsettings.json` contains `ConnectionStrings`, `Jwt` (issuer/audience/key/expiry),
`RateLimiting`, and `Serilog` sections. **The values committed here are intentionally dev-only**
and called out as such. In production these belong in environment variables, user-secrets, or a
secret store — never in source control. The JWT signing key must be at least 32 characters.

---

## Tests

```bash
dotnet test
```

29 xUnit tests covering validators, JWT issuance, the auth flow (success/failure + audit),
pagination math and shape parity, the audit interceptor, controller behaviour/authorization, and a
`WebApplicationFactory` integration test (login → create → list, plus the unauthenticated `401`).
Tests run against **SQLite in-memory** (real relational behaviour, no Docker needed for the suite).

---

## Known limitations / future work

- No token refresh / revocation (60-minute access tokens only).
- No public user registration — users are seeded; an admin user-management surface would come next.
- Audit `Details` is a JSON string; a typed audit query/report API could be added.
- Single-instance rate limiting (in-memory); a distributed store (Redis) would be needed at scale.
