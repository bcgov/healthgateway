# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> Also read `healthgateway/AGENTS.md` (repo root) — it has build/lint/test commands and code style rules. This file focuses on architecture and context that requires reading multiple components to understand.

## Repository Layout

The working directory is `healthgateway/Apps`. The broader repo:
- `healthgateway/Apps` — all .NET services, shared libraries, and the Vue WebClient
- `healthgateway/Testing` — integration tests (`.NET`) and Cypress functional/e2e tests
- `healthgateway/Tools` — Helm charts, Kong config, Keycloak setup, local dev tooling
- `healthgateway/Build` — Azure DevOps pipeline YAML

## Commands

All paths below are relative to the repo root (`healthgateway/`).

### .NET

```bash
# Build entire solution
dotnet build Apps/HealthGateway.sln

# Run all unit tests
dotnet test Apps/HealthGateway.sln

# Run tests for a single service (faster)
dotnet test Apps/GatewayApi/test/unit/GatewayApiTests.csproj

# Run a single test by name
dotnet test Apps/HealthGateway.sln --filter FullyQualifiedName~Namespace.ClassName.TestName
```

### Vue WebClient

```bash
cd Apps/WebClient/src/ClientApp
npm run dev          # Vite dev server on port 5002
npm run build        # type-check + lint + production build
npm run lintcheck    # ESLint + Prettier check (no write)
npm run lint         # ESLint --fix + Prettier --write
```

### Admin Blazor App

The Admin app has a Blazor WebAssembly client (`Admin/Client`) hosted by an ASP.NET Core server (`Admin/Server`). Build and run via the server project.

### Cypress E2E Tests

```bash
# WebClient e2e
cd Testing/functional/tests && npm run e2e

# Admin e2e
cd Apps/Admin/Tests/Functional && npm run e2e

# Single spec
npx cypress run --spec "cypress/integration/user/emailValidation.js"
```

## Architecture

### Request Flow

```
User browser
  └── Vue SPA (WebClient, Keycloak/BCSC auth)
        └── GatewayApi  (main orchestrating API)
              ├── Database (EF Core → PostgreSQL)  user profiles, comments, notes, emails
              ├── AccountDataAccess               PHSA account/identity API
              └── Domain microservices (Refit HTTP clients):
                    Immunization, Medication, Laboratory,
                    Encounter, ClinicalDocument, Patient
                        └── External systems:
                              PHSA health data APIs (v2 token exchange)
                              HCIM Client Registry (SOAP)
                              HNClient (HL7 → HN backend for medications)
                              ODRProxy (Online Drug Repository)
```

The **Admin** portal (Blazor WASM + ASP.NET Core) talks directly to the same `Database` delegates and shared services rather than going through `GatewayApi`.

### Key Services

| Project | Role |
|---|---|
| `GatewayApi` | Main API consumed by the Vue WebClient; orchestrates user data |
| `WebClient` | ASP.NET Core host for the Vue 3 SPA; dev uses Vite proxy on port 5002 |
| `Admin/Server` + `Admin/Client` | Admin portal — Blazor WASM hosted by ASP.NET Core |
| `JobScheduler` | Hangfire server for background jobs (email queues, drug data loads, DB migrations, notification sync) |
| `Database` | EF Core `GatewayDbContext`, migrations, and repository-pattern delegates shared by all services |
| `Common` | Shared infrastructure: auth, Redis caching, Service Bus messaging, auditing, Serilog, OpenTelemetry, Hangfire config, Swagger, Refit client factories |
| `CommonData` | Shared data models and validation |
| `AccountDataAccess` / `PatientDataAccess` | PHSA account and health-data access layers used by GatewayApi |

### Cross-Cutting Infrastructure (in `Common`)

- **Caching**: Redis via StackExchange.Redis; injected as `ICacheProvider`
- **Messaging**: Azure Service Bus with a DB outbox pattern as fallback
- **Background jobs**: Hangfire + `Hangfire.PostgreSql`
- **Observability**: Serilog (structured logs → Seq / App Insights) + OpenTelemetry (traces + metrics)
- **Auth**: Keycloak (BC Login Proxy, realm `health-gateway-gold`) with BCSC as IdP; PHSA services use a v2 token-exchange pattern
- **Report generation**: CDOGS (Common Document Generation Service) for docx/xlsx downloads

### Data Access Pattern

Services use a layered delegate pattern:
- `IXxxDelegate` in `Database` — direct EF Core DB access
- `IXxxService` in each service — business logic, calls delegates + other services
- External API clients are Refit interfaces defined in `Common` (e.g., `IImmunizationApi`, `IPhsaApi`)

### .NET Project Conventions

- SDK: .NET 10, `net10.0`, C# 14, nullable enabled
- Solution-wide MSBuild properties in `Apps/Directory.Build.props`
- StyleCop enforced; XML docs are **not** required; license header configured
- Test projects live at `<Service>/test/unit/`; xUnit + Moq + FluentAssertions/Shouldly
- Nullability warnings (`CS8600`, `CS8602`, `CS8603`) are compile errors

### Vue WebClient Conventions

- Vue 3 + TypeScript + Vite; Vuetify 3 for UI components
- Pinia for state management; Vue Router 5
- Keycloak-js for OIDC; Vuelidate for forms; Axios for HTTP
- ESLint + Prettier enforced; `simple-import-sort` for imports