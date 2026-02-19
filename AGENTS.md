# AGENTS.md

This repository is a multi-app .NET + web client monorepo. Use this guide to
find build/lint/test commands and to follow the local code style.

If you are an agent, read these files first:
- `healthgateway/.editorconfig`
- `healthgateway/Apps/Directory.Build.props`
- `healthgateway/Apps/stylecop.json`
- `healthgateway/Apps/WebClient/src/ClientApp/eslint.config.mjs`
- `healthgateway/Apps/Admin/Tests/Functional/eslint.config.mjs`
- `healthgateway/Testing/functional/tests/eslint.config.mjs`

No Cursor rules or Copilot instructions were found in this repo.

## Repo Layout (high level)
- `healthgateway/Apps` contains .NET services, shared
  libraries, and the WebClient.
- `healthgateway/Testing` contains integration and
  functional (Cypress) tests.
- `healthgateway/Tools` contains dev tooling and
  infra helpers.

## Tooling and Versions
- .NET SDK pinned by `healthgateway/Apps/global.json`
  (`10.0.100`, rollForward latestMajor).
- C# projects target `net10.0` and nullable reference types are enabled.
- Web client uses Vite + Vue 3 + TypeScript and ESLint + Prettier.
- Cypress functional tests use ESLint + Prettier.

## Build, Lint, Test Commands
All commands below are run from the repo root unless noted.

### .NET builds
```bash
dotnet build healthgateway/Apps/HealthGateway.sln
```

### .NET unit tests (solution)
```bash
dotnet test healthgateway/Apps/HealthGateway.sln
```

### .NET integration tests
```bash
dotnet test healthgateway/Testing/integration/integration.sln
```

### Run a single .NET test
```bash
dotnet test healthgateway/Apps/HealthGateway.sln \
  --filter FullyQualifiedName~Namespace.ClassName.TestName
```
Notes:
- You can also filter by trait: `--filter Category=Unit`.
- For a specific test project, pass its `.csproj` path instead of the `.sln`.

### Web client (Vue) build
```bash
cd healthgateway/Apps/WebClient/src/ClientApp
npm run build
```

### Web client lint
```bash
cd healthgateway/Apps/WebClient/src/ClientApp
npm run lintcheck
```

### Web client lint + fix
```bash
cd healthgateway/Apps/WebClient/src/ClientApp
npm run lint
```

### Cypress functional tests (WebClient)
```bash
cd healthgateway/Testing/functional/tests
npm run e2e
```

### Cypress functional tests (Admin)
```bash
cd healthgateway/Apps/Admin/Tests/Functional
npm run e2e
```

### Run a single Cypress spec
```bash
cd healthgateway/Testing/functional/tests
npx cypress run --spec "cypress/integration/user/emailValidation.js"
```

```bash
cd healthgateway/Apps/Admin/Tests/Functional
npx cypress run --browser chrome --spec "cypress/integration/e2e/authentication/login.cy.js"
```

## Code Style and Conventions

### Shared formatting rules (all files)
- Indent with 4 spaces.
- LF line endings.
- Trim trailing whitespace.
- Ensure a final newline.
- UTF-8 charset.

### C# and .NET conventions
Source of truth:
- `healthgateway/.editorconfig`
- `healthgateway/Apps/Directory.Build.props`
- `healthgateway/Apps/stylecop.json`

Key points to follow:
- Nullable reference types are enabled; avoid null warnings and keep builds clean.
- `using` directives are placed inside the namespace.
- Prefer explicit types over `var` (`csharp_style_var_*` are false suggestions).
- Private instance and static fields use `camelCase`.
- Private `const` and private static `readonly` fields use `PascalCase`.
- Keep line length under 200 (ReSharper config).
- Braces are required for `if`, `for`, `foreach`, `while`.
- Prefer `nameof(...)` in argument checks and exceptions.
- Favor `Any()` over `Count()` for existence checks.
- Avoid general `catch (Exception)` unless you immediately rethrow or wrap.
- Do not use insecure deserialization or weak crypto (see CA23xx, CA535x rules).
- Avoid hardcoded secrets and security protocol downgrades.

StyleCop notes:
- XML headers are disabled; standard license header is configured if needed.
- Documentation rules are relaxed for tests and store effects/reducers.

### TypeScript / Vue conventions (WebClient)
Source of truth:
- `healthgateway/Apps/WebClient/src/ClientApp/eslint.config.mjs`

Key points:
- Use ESLint + Prettier; do not fight the formatter.
- `simple-import-sort` enforces import and export ordering.
- Unused imports are warnings; remove them.
- Unused variables are allowed only when prefixed with `_`.
- `any` is allowed but discouraged (warn); prefer precise types.

### Cypress test conventions
Source of truth:
- `healthgateway/Testing/functional/tests/eslint.config.mjs`
- `healthgateway/Apps/Admin/Tests/Functional/eslint.config.mjs`

Key points:
- Use `simple-import-sort` and remove unused imports.
- ESLint runs over `cypress/**/*.js`.

## Error Handling and Logging
- Validate public method arguments where appropriate and use `ArgumentNullException`.
- Preserve stack traces when rethrowing (`throw;`, not `throw ex;`).
- Use structured logging and avoid expensive string formatting when logging is disabled.
- Treat security analyzer warnings seriously; they are elevated to warnings or errors.

## Tests and Quality Gates
- C# nullability warnings (`CS8600`, `CS8602`, `CS8603`) are errors.
- StyleCop `SA1000` spacing is an error.
- Some analyzers are disabled in tests; do not rely on that in production code.

## Agent Notes
- Many apps are independent; target the closest `.sln` or `.csproj` for faster builds.
- Prefer narrow test execution (`--filter`) to keep CI cycles fast.
- Keep changes localized and follow existing conventions in each app.
