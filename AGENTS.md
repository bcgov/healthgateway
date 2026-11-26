# AGENTS.md

This repository is a multi-app .NET + React Native client monorepo. Use this guide to
find build/lint/test commands and to follow the local code style. All paths below are
relative to the repo root.

If you are an agent, read these files first:
- `.editorconfig`
- `Apps/Directory.Build.props`
- `Apps/stylecop.json`
- `Apps/WebClient/src/ClientApp/eslint.config.mjs`
- `Apps/Admin/Tests/Functional/eslint.config.mjs`
- `Testing/functional/tests/eslint.config.mjs`

## Repo Layout (high level)
- `Apps` contains .NET services, shared libraries, and the ClientApp.
- `Apps/WebClient/src/ClientApp` is the Expo + React Native client for iOS, Android, and web.
- `Apps/WebClient/src/ClientApp.old` is the retired Vue client; ignore it unless asked.
- `Testing` contains integration (.NET) and functional (Cypress) tests.
- `Tools` contains Helm charts, Kong config, Keycloak setup, and local dev tooling.
- `Build` contains Azure DevOps pipeline YAML.

## Tooling and Versions
- .NET SDK pinned by `Apps/global.json` (`11.0.100`, rollForward latestMajor).
- C# projects target `net11.0` with C# 15, and nullable reference types are enabled.
- ClientApp uses Expo + React Native + TypeScript and ESLint + Prettier.
- Cypress functional tests use ESLint + Prettier.

## Build, Lint, Test Commands
All commands below are run from the repo root unless noted.

### .NET builds
```bash
dotnet build Apps/HealthGateway.sln
```

### .NET unit tests
```bash
# Entire solution
dotnet test Apps/HealthGateway.sln

# A single service (faster)
dotnet test Apps/GatewayApi/test/unit/GatewayApiTests.csproj
```

### .NET integration tests
```bash
dotnet test Testing/integration/integration.sln
```

### Run a single .NET test
```bash
dotnet test Apps/HealthGateway.sln \
  --filter FullyQualifiedName~Namespace.ClassName.TestName
```
Notes:
- You can also filter by trait: `--filter Category=Unit`.
- For a specific test project, pass its `.csproj` path instead of the `.sln`.

### ClientApp
See `Apps/WebClient/src/ClientApp/AGENTS.md` for its build, lint, unit test, and
Maestro commands.

### Cypress functional tests (ClientApp web)
```bash
cd Testing/functional/tests
npm run e2e
```

### Cypress functional tests (Admin)
```bash
cd Apps/Admin/Tests/Functional
npm run e2e
```

### Run a single Cypress spec
```bash
cd Testing/functional/tests
npx cypress run --spec "cypress/integration/ui/user/emailValidation.js"
```

```bash
cd Apps/Admin/Tests/Functional
npx cypress run --browser chrome --spec "cypress/integration/e2e/authentication.cy.js"
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
- `.editorconfig`
- `Apps/Directory.Build.props`
- `Apps/stylecop.json`

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

### TypeScript / React Native conventions (ClientApp)

See `Apps/WebClient/src/ClientApp/AGENTS.md`.

### Cypress test conventions
Source of truth:
- `Testing/functional/tests/eslint.config.mjs`
- `Apps/Admin/Tests/Functional/eslint.config.mjs`

Key points:
- Use `simple-import-sort` and remove unused imports.
- ESLint runs over `cypress/**/*.js`.
- In both Cypress folders, `npm run lint` only checks (`--max-warnings 0`) and
  `npm run lintfix` writes fixes. This is the reverse of the ClientApp, where `lint` writes.

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
- **Hard rule: never commit or push on behalf of the user.** Do not run `git commit`,
  `git push`, or anything that creates commits or updates a remote (including amending,
  merging, rebasing, tagging, or opening PRs). Leave changes in the working tree for the
  user to review, commit, and push themselves.
- Many apps are independent; target the closest `.sln` or `.csproj` for faster builds.
- Prefer narrow test execution (`--filter`) to keep CI cycles fast.
- Keep changes localized and follow existing conventions in each app.
