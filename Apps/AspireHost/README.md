# AspireHost

Aspire AppHost that orchestrates the full local Health Gateway development environment:

- **PostgreSQL 15** (container `GatewayDB`), bootstrapped with the credentials, database,
  and port from the `ConnectionStrings:GatewayConnection` user secret (`localhost:5432`
  with the standard secret).
  When the data volume is first created, `postgres-init/00_CreateExtensions.sql` adds the
  `uuid-ossp` extension (as `Tools/Dev/Postgres/init/00_SetupDevDB.sql` does), which the
  audit trigger functions in the migrations depend on.
- **DbMigrations** — initializes/updates the schema by running
  `dotnet ef database update --project ../Database/src` from the DBMaintainer directory
  (DBMaintainer is the migrations startup project). This is the only database
  initialization; all apps wait for it to finish before starting.
- **Redis 6.2** (container `GatewayCache`), mirroring the `RedisConnection` user secret's
  port and password, if one is set (`localhost:6379` with the standard secret).
- **Apps** — everything except Admin: Patient, GatewayApi, Immunization, Medication,
  Laboratory, Encounter, ClinicalDocument, JobScheduler, and WebClient (SpaProxy launches
  the Vite dev server). Each app keeps the port from its own launch profile (GatewayApi
  3000, WebClient 3025, JobScheduler 5005, etc.), so existing inter-service URLs work
  unchanged. Drug data loads via JobScheduler's recurring Hangfire jobs.

The containers are removed when the AppHost stops; their data persists in the
`gatewaydb.local` and `gatewaycache.local` Docker volumes — the same volumes used by
`Tools/Dev/Postgres/docker-compose.yml`, so existing developers keep their data when
switching between the two.

The AppHost logs through Serilog with the same defaults as the WebClient (see the
`Serilog` section in `appsettings.json`).

## Configuration

The AppHost shares the developer user secrets used by the apps (same `UserSecretsId`) and
requires two of its keys — no credentials live in source:

- `ConnectionStrings:GatewayConnection`
- `RedisConnection`

It injects those values into every app as `HealthGateway_`-prefixed environment variables,
which `ProgramConfiguration` loads last, so they override user secrets and
`appsettings.local.json`. All other configuration (Keycloak secrets, PHSA endpoints, etc.)
still comes from user secrets as before.

## Prerequisites

- Docker Desktop running
- `dotnet-ef` global tool (`dotnet tool install --global dotnet-ef`)
- Stop the old compose stack first if it's running, since both use ports 5432/6379:
  `docker compose -f Tools/Dev/Postgres/docker-compose.yml down`

## Run and view the dashboard

```console
dotnet run --project Apps/AspireHost
```

The console prints a dashboard login link, e.g.

```console
Login to the dashboard at https://localhost:17100/login?t=<token>
```

Open that link (the token logs you in automatically). The dashboard at
`https://localhost:17100` shows every resource with its state, endpoints, console logs,
environment, and telemetry, and has start/stop/restart controls per resource. If you lose
the link, the token is reprinted in the AppHost console output on each run.

## Resetting local state

```console
docker volume rm gatewaydb.local gatewaycache.local
```

The next AppHost run re-creates the containers and re-runs the migrations from scratch.
