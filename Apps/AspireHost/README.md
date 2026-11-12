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
- **Apps** — Patient, GatewayApi, Immunization, Medication, Laboratory, Encounter,
  ClinicalDocument, JobScheduler, WebClient (SpaProxy launches the Vite dev server),
  and Admin (Blazor WASM portal). Each app keeps the port from its own launch profile
  (GatewayApi 3000, WebClient 3025, JobScheduler 5005, Admin 5027, etc.), so existing
  inter-service URLs work unchanged. Drug data loads via JobScheduler's recurring
  Hangfire jobs.
- **ClientApp** — Metro, the Expo dev server (`npm run dev`, port 5002), started straight
  away so WebClient's SpaProxy doesn't start its own. The web app (http://localhost:5002)
  and the iOS/Android dev builds load from it, with hot reload; its log is the resource's
  console in the dashboard. Stop any `expo start` / `expo run:ios` of your own first, since
  it uses port 5002. Restart the resource after changing `app.config.ts`.
- **MobileApp** and **MobileBuild** — the dev app on an Android emulator or an iOS
  simulator, started from the dashboard (Start; they don't run on their own). Two settings
  pick the device; set them as environment variables (or in user secrets / appsettings):
  - `HG_MOBILE_PLATFORM`: `android` (default) or `ios` (macOS only).
  - `HG_MOBILE_DEVICE`: the AVD or simulator by name, e.g. `Pixel_10_Pro_XL` or
    `iPhone 18 Pro Max` (a simulator UDID also works). Without one: a running one, else
    the first AVD, or the newest iOS's first iPhone.

  One that isn't running is started (iOS: shown in Xcode 27's Device Hub, or the
  Simulator app of an older Xcode), and shut down when Aspire stops; one that was already
  running is left alone. Restarting a resource in the dashboard keeps the device up.
  **MobileBuild** builds and installs the dev app, then opens it: needed once per
  emulator or simulator, then after native changes (a native package, a plugin, native
  fields in `app.config.ts`); Android needs `JAVA_HOME` (and on Windows, see
  [Android builds on Windows](#android-builds-on-windows)). **MobileApp** opens the installed
  dev app on Metro, once WebClient is up. A development build starts on the Local
  environment, these services; Security → Environment in the app switches it.
  Both run `Apps/WebClient/src/ClientApp/scripts/mobile-dev.js` (`open` / `build`), which
  also works by hand with the same two variables.

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

### Android builds on Windows

The native (CMake) part of **MobileBuild**'s Android build creates paths longer than
Windows' 260-character limit, failing with a ninja `mkdir(...): No such file or directory`
error. Two one-time changes fix this:

- Enable long paths, from an elevated PowerShell, then sign out or restart:

  ```powershell
  New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name LongPathsEnabled -Value 1 -PropertyType DWORD -Force
  ```

- Replace the ninja bundled with the Android SDK's CMake, as versions before 1.12 don't
  support long paths. The Android Gradle Plugin uses the `ninja.exe` beside `cmake.exe`
  (not one on `PATH`), e.g. `%LOCALAPPDATA%\Android\Sdk\cmake\3.22.1\bin\ninja.exe`.
  Replace it with `ninja.exe` from `ninja-win.zip` in one of the more recent
  [ninja releases](https://github.com/ninja-build/ninja/releases).
  Reinstalling or adding a CMake version through the SDK Manager will bring back an old ninja.

**MobileBuild** checks each SDK CMake's ninja version before building and stops with an
error naming any that is older than 1.12. After fixing an earlier failure, delete
`node_modules/<package>/android/.cxx` and `android/app/.cxx` in the ClientApp folder so
the native build starts clean.

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
