# PlayLeague

A sports league management platform for organizing leagues, teams, events, schedules, and rosters.

## Stack

| Layer | Tech |
|---|---|
| Backend | ASP.NET Core (.NET 10), C#, MediatR (CQRS), EF Core, PostgreSQL |
| Frontend | Vue 3, TypeScript, Vite, Pinia, Tailwind CSS 4 |
| Auth | JWT (Bearer tokens) |

## Features

- League and team management
- Roster management with invitations
- Event scheduling and RSVP
- Game schedules and venues
- Practice planner
- Notifications
- Admin panel

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- PostgreSQL running on `localhost:5432`

## Getting Started

### Backend

```bash
cd backend
dotnet run
```

The API runs on `https://localhost:7xxx` / `http://localhost:5xxx`. On first run in Development mode, the database is seeded automatically.

**Connection string** (edit `appsettings.Development.json`):
```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=playleague;Username=postgres;Password=admin"
}
```

**Run migrations:**
```bash
dotnet ef database update
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Runs on `http://localhost:5173`. The Vite dev server proxies API calls; CORS is configured to allow this origin.

## Project Structure

```
backend/
  Controllers/     # HTTP endpoints
  Features/        # CQRS handlers (Auth, Leagues, Teams, Events, ...)
  Data/            # EF Core DbContext, migrations, seeder
  Models/          # Domain entities
  Services/        # JWT token service
  Middleware/      # Global exception handling

frontend/
  src/
    pages/         # Route-level components
    components/    # Shared UI components
    stores/        # Pinia state
    api/           # Axios API clients
    composables/   # Reusable Vue composition functions
```
