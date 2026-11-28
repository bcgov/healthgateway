# Agent Instructions for ClientApp

**Rules and information for the Health Gateway React Native client.**

Read the repo-root `AGENTS.md` for agent notes and the greater context.

## Stack

- Expo + React Native + react-native-web: one TypeScript codebase for iOS, Android, and web.
- expo-router for routing (`src/app`), Zustand for state, NativeWind (Tailwind) for styling, and Luxon for dates.
- OIDC through Keycloak: keycloak-js on web and expo-auth-session on native.
- Jest (jest-expo) and Testing Library for unit and component tests in `tests/unit`, Maestro for native end-to-end tests, and Cypress (`Testing/functional/tests`) for web end-to-end tests.
- On web, the WebClient ASP.NET Core app serves the exported bundle (`dist/`). In development it proxies to the Expo dev server on port 5002.

## Commands

Run from this folder (`Apps/WebClient/src/ClientApp`). `README.md` documents every script.

```bash
npm run dev                      # Expo dev server on port 5002 (not alongside Aspire's ClientApp, which also uses 5002)
npm run lintcheck                # ESLint (--max-warnings 0) + Prettier check, no writes
npm run lint                     # ESLint --fix + Prettier --write
npm run build                    # typecheck + lintcheck + web export to dist/
npm test                         # Jest unit and component tests
npm test -- --selectProjects web # one Jest project (native or web)
npm run test:coverage            # Jest with an 80% coverage gate on stores, services, navigation, utils, and hooks
npm run e2e:ios                  # Maestro flows on iOS
npm run e2e:android              # Maestro flows on Android
```

`build` already runs `typecheck` and `lintcheck`, so there's no need to run them separately first.

Jest has two projects so that platform-specific modules are both exercised: unit tests (`*.test.ts`) run on the native project only, and component tests (`*.test.tsx`) run on both native and web.

## Project Layout Notes

- `src/app/`: Expo Router routes. Keep these files thin; screens live in `src/views/`.
- `src/components/`: UI primitives (`common/`) and the site shell (`site/`).
- `src/theme/`: Design tokens, also used by Tailwind.
- `modules/hg-secure-vault/`: native module for biometric-locked encryption.
- `modules/hg-legacy-cleanup/`: native module that removes the data left by the old Health Gateway apps (see `legacy.md`).
- `tests/unit/`: Jest tests, mirroring `src/`. Shared helpers are in `tests/unit/support/`.
- `tests/maestro/`: Maestro flows, grouped by feature area.

Path aliases: `@/*` maps to `src/*`, `@/test/*` to `tests/unit/support/*`, and `@/assets/*` to `assets/*`.

Ignore `../ClientApp.old/` by default when searching or editing; it's the retired Vue client, kept for reference only.

## Code Conventions

Source of truth: `eslint.config.js`, `.prettierrc`, and `tsconfig.json`.

- `lintcheck` runs with `--max-warnings 0`, so warnings fail the check: remove unused imports and variables, and avoid `any` in favour of precise types.
- Unused function parameters are allowed only when prefixed with `_`.
- Type-aware `typescript-eslint` rules are enabled. Mark promises that are deliberately not awaited with `void`.
- Platform-specific modules use `.web.ts(x)`, `.ios.ts(x)`, `.android.ts(x)`, and `.native.ts(x)` suffixes.

### Comments

#### When and why to comment

1. **Comment only when the code can't say it.** A comment explains _why_: business rules, backend or third-party constraints, ordering or initialization dependencies, rollback/guard logic, or magic numbers. If a comment would only explain _what_ the code does, try a clearer name or a small extracted function first.
2. **Don't add noise.** Don't narrate obvious steps, add section markers, or restate names or types without adding context. Don't leave commented-out code.
3. **Keep comments short.** A single sentence or sentence fragment is the default. Use a multi-line comment only for a real constraint that needs the space.
4. **Keep comments accurate.** When you change or copy code, update or delete its comments, because a wrong comment is worse than none.
5. **Prefix platform-specific notes.** Start the comment with `Native:`, `iOS:`, `Android:`, or `Web:` when it applies to only one platform and the file doesn't already make that clear.
6. **Follow these rules even when nearby code differs.** However, do not rewrite existing comments outside the code you're changing.

#### `//` comments

Use `//` for everything except the TSDoc cases below:

- A _why_ comment inside a function body, on the line above the code it explains.
- A short note at the end of a line, e.g. on a guard clause or a constant.
- A note on a non-exported function or constant.

Write `// ` followed by a sentence or fragment. A capital letter and periods are required only when the comment has more than one sentence. Avoid `/* */` wherever possible.

Examples:

```ts
// override default margin that centres it in the window
node.style.setProperty("margin", "0");

const maxRetries = 3; // the API gateway rate-limits after the fourth attempt
```

#### `/** */` TSDoc comments

Use TSDoc for exported functions, models, and other code meant to be accessed in other files, so their documentation appears in editor hovers.

- Start with a third-person present-tense summary sentence ending in a period ("Validates…", "Returns…").
- For functions, add `@param name Description.` for every parameter and `@returns Description.` for non-void returns. Don't add type annotations, because TypeScript already provides them.
- Note defaults in parentheses ("(defaults to true)").
- Never leave a tag empty.
- A summary with no tags can be written on one line: `/** Formats a phone number as (###) ###-####. */`.

Example:

```ts
/**
 * Creates a new DateWrapper from a string containing a date. Any time information is discarded.
 * @param dateString Date string in "yyyy-MM-dd" format.
 * @returns A new DateWrapper object for the given date at the start of the day.
 */
public static fromIsoDate(dateString?: string | null): DateWrapper {
```

#### Comments in unit tests

- **Comment the test setup, not the assertions.** Good reasons include a limitation of the test environment, a non-obvious fixture value, or why a timer is advanced.
- **TSDoc is not needed** for unit test utilities or helpers.

Example:

```ts
// jsdom never finishes the modal's exit animation, so closing is covered by Cypress
it("opens the tour from the menu", async () => {
```
