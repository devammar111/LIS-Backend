# LIS Lab Order Application

A Laboratory Information System (LIS) demo that lets users submit lab orders and view submitted orders. The backend is a .NET 8 Web API and the frontend is an Angular app.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Angular CLI 19+](https://angular.dev/tools/cli) (optional; `npm install` in `frontend` is enough to run the app)

## How to run

### 1. Start the API

```powershell
cd "C:\Work Folder\Mine Workspaces\LIS Dot NET 8 and Angular\backend\LIS.Api"
dotnet restore
dotnet run
or you can run on Visual studio on http protocole. Otherwise you can observe CORS error.
```

The API runs at `http://localhost:5062`. Swagger UI is available at `/swagger`.

### 2. Start the Angular app

Open a second terminal:

```powershell
cd "C:\Work Folder\Mine Workspaces\LIS Dot NET 8 and Angular\frontend"
npm install
npm run start
```

The UI runs at `http://localhost:4200`.

**Important:** The backend must be running before you use the frontend. Start the API first, then start Angular. The Angular dev server proxies `/api` requests to `http://localhost:5062`.

## Persistence choice

**In-memory store** (`InMemoryOrderRepository`)

This project uses an in-memory repository backed by a `ConcurrentBag<LabOrder>` registered as a singleton in the DI container.

**Why this choice**

- Zero setup — no database install or connection strings
- Fast to run and demo locally
- Keeps the focus on API design, validation, and layered architecture

**Tradeoff**

- All submitted orders are lost when the API process restarts
- Not suitable for production or multi-instance deployment without adding shared storage

For a production LIS, I would replace the repository with EF Core + SQL Server (or PostgreSQL), add audit trails, and enforce authorization by role.

## Architecture

```
Controller  →  Service  →  Repository
(HTTP only)    (rules)      (data access)
```

- `OrdersController` — HTTP request/response mapping only
- `OrderService` — validation and domain rules
- `InMemoryOrderRepository` — persistence

## API

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders` | Create a lab order |
| GET | `/api/orders` | List orders sorted by collection date descending |
| GET | `/api/orders?priority=high` | List STAT orders only |

Validation failures return `400 Bad Request` with structured field errors:

```json
{
  "errors": {
    "patientName": ["Patient name is required."],
    "collectionDate": ["Collection date cannot be in the past."]
  }
}
```
