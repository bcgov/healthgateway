# Health Gateway UI

React Native (Expo) rewrite of the Health Gateway client for iOS, Android and responsive web.

## Prerequisites

- Node 24 and npm (Expo 58 and React Native 0.88 need Node 22.13+ or 24.3+; CI uses 24.x)
- Xcode with an iOS simulator, CocoaPods (`pod`)
- Android Studio with an emulator, JDK 17 (`ANDROID_HOME` set)

## Run

```bash
npm install                 # everything the web build needs; native tooling is only for ios/android
npm run web        # http://localhost:5002
npm run ios        # prebuilds ./ios (git-ignored), installs pods, builds and launches on a simulator
npm run android    # same for ./android and an emulator
```

Native builds choose their environment at run time: development builds start on Development, others on Production. The
Security page's Environment section switches it, after a confirmation.

`npm run [ios/android] -- --device` lets you pick a specific simulator or physical device. iOS needs a free Apple developer signing identity in Xcode.
Expo Go is **not** supported: the native OIDC redirect uses the custom `healthgateway://` scheme, which needs a real build.

## Release builds on a phone

A release build carries the app's code inside it, so the phone doesn't need Metro or your laptop once it's installed. These are
local builds, with no EAS or Expo account.

Plug the phone in over USB, then run these in your own terminal, from this folder (`Apps/WebClient/src/ClientApp`). Rebuild each time you want code changes
on the phone.

**iOS** (Developer Mode must be on, under Settings > Privacy & Security):

```sh
npx expo run:ios --configuration Release --device
```

- **Signing:** needs a signing identity in Xcode. `ios/` is git-ignored, so on a fresh checkout set `APPLE_TEAM_ID` before the first
  build, or choose the team in Xcode. A free Apple account's build stops opening after 7 days; a paid team's lasts as long as its
  provisioning profile.
- **If it stalls at "Connecting":** the build has already finished, so install it directly. Get the ID from
  `xcrun devicectl list devices`, and the `.app` path from the build output:
  `xcrun devicectl device install app --device <udid> <path to .app>`.

**Android** (USB debugging must be on, under Developer options; check the phone is listed by `adb devices`):

```sh
npx expo run:android --variant release --device
```

- **Signing:** release builds use the debug key from `android/app/build.gradle`. That's fine on your own phone, but a store release
  needs a real key.
- **Installing the same build on another phone:** `adb -s <device> install -r android/app/build/outputs/apk/release/app-release.apk`.

**What's different from a development build:**

- **It replaces the dev build:** both use the `ca.bc.gov.healthgateway.dev` ID unless `APP_ENV` says otherwise, so a phone holds one
  or the other. Your login and saved copy normally carry over.
- **Environment:** a release build starts on Production unless that phone already chose another, and doesn't offer Local. Normal use
  on Production counts in Production analytics.
- **Errors:** no red or yellow error boxes, so reproduce problems on a dev build.

## Configuration

Runtime configuration is fetched from the Health Gateway `/configuration` endpoint on every platform.
Native uses the selected environment's URL (`src/config/environments.ts`). Web uses `EXPO_PUBLIC_WEB_CLIENT_BASE_URI` +
`configuration`, as the web client does: `.env.development` sets it to `http://localhost:3025/` for local runs, and without it the app asks the server that
hosted it. `APP_ENV=dev|test|prod` selects the native app name,
bundle id / package (`ca.bc.gov.healthgateway[.dev|.test]`) and is read by `app.config.ts` at build time.

## Keycloak prerequisites (team-owned, dev realm `health-gateway-gold`, client `hg`)

The dev server runs on port **5002** because `http://localhost:5002` is already registered on the `hg` client for web
development. Only the native custom-scheme entries still need to be added:

| Setting                         | Add                                                                                                                                                        |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Valid Redirect URIs             | `http://localhost:5002/*` (the check-sso fallback returns to whichever page was loaded), `healthgateway://loginCallback`, `healthgateway://logoutComplete` |
| Valid Post Logout Redirect URIs | `http://localhost:5002/logoutComplete`, `healthgateway://logoutComplete`                                                                                   |
| Web Origins                     | `http://localhost:5002`                                                                                                                                    |

## Authentication design

- Web: `keycloak-js` (same as the existing web client), PKCE S256. Tokens live only in memory, as in the old Vue app, so
  every page load runs `check-sso` again. It first tries a hidden iframe (`public/silent-check-sso.html`). When the browser
  blocks third-party cookies (Safari, Firefox strict, Brave) the iframe cannot see Keycloak's session, and keycloak-js falls
  back to a full-page `prompt=none` redirect that returns to the same page. That fallback runs only when this browser was
  signed in at its last load (a non-secret `hg.auth.hadSession` flag in `localStorage`, cleared at sign-out), so anonymous
  visitors never leave the site on load; a tab that just redirected does not redirect again, so it cannot loop.
  Callbacks are `/loginCallback` and `/logoutComplete`.
- Native: `expo-auth-session` PKCE S256 with an **ephemeral** auth session (`preferEphemeralSession: true`), so iOS shows no
  "wants to use … to sign in" sheet and no cookies are shared with Safari. Tokens live in `expo-secure-store`.
  Logout posts the refresh token to Keycloak's end-session endpoint; no browser is opened.
- The identity-provider list, feature toggles and service endpoints all come from `/configuration`.

## Analytics (Snowplow)

Same collector, app id, schema and event vocabulary as the web client (`src/constants/tracking.ts` is a port of
its `plugins/extensions.ts`). It is on unless switched off, locally too, as the old Vue client was; these are the defaults
(`.env.example`), and `EXPO_PUBLIC_SNOWPLOW_ENABLED=false` in your `.env` switches it off:

```
EXPO_PUBLIC_SNOWPLOW_ENABLED=true
EXPO_PUBLIC_SNOWPLOW_COLLECTOR=spt.apps.gov.bc.ca
EXPO_PUBLIC_SNOWPLOW_APP_ID=Snowplow_standalone
```

Web uses `@snowplow/browser-tracker` (page view per route, activity ping, link clicks); native uses
`@snowplow/react-native-tracker` (page view per route, addressed on the selected environment's host). When disabled, events are only logged at debug level.

## End-to-end (Maestro)

Flows live in `tests/maestro/`, grouped by function:

| Folder          | Covers                                                                                                  |
| --------------- | ------------------------------------------------------------------------------------------------------- |
| `auth/`         | Register starting a login, the Virtual Patient Portal hand-off page                                     |
| `home/`         | Home's tiles and the Add Quick Link dialog                                                              |
| `records/`      | Health Records, Dependents, Services, Download, and the protective word (a second account)              |
| `account/`      | Profile and Feedback                                                                                    |
| `navigation/`   | The logo, the hiding header, the tab bar and the signed-out gear menu                                   |
| `settings/`     | Dark Mode, the Security page and switching Environment                                                  |
| `connectivity/` | Offline and pull to refresh                                                                             |
| `onboarding/`   | The first-launch intro and security step                                                                |
| `subflows/`     | Steps the flows call (start signed in or out, skip the intro, switch environment); not run on their own |
| `tools/`        | Helpers run by hand (accept or cancel an "Open in" sheet, scroll, capture the login sheet); not tests   |

`config.yaml` makes `maestro test tests/maestro` run every group, leaving out `subflows/` and `tools/`. Flows only open forms and
cancel them: none adds, removes, sends or resets anything, except the two tagged `resetsApp` (`settings/environment` and
`onboarding/onboarding`), which switch environment. That is a Reset App, so it logs you out.

**Everything in one go:**

```sh
npm run e2e:ios                                              # the booted simulator, or iPhone 16 Pro
npm run e2e:ios -- "iPhone Duo"                              # a simulator by name or id, booted if needed
npm run e2e:android                                          # the running emulator, or the first AVD
HG_ANDROID_AVD=Pixel_10_Pro_XL npm run e2e:android           # an emulator by AVD, started if needed
```

Each one uses the booted simulator or running emulator, or starts one (`HG_IOS_SIMULATOR`, default "iPhone 16 Pro";
`HG_ANDROID_AVD`, default the first AVD listed). It always starts a fresh Metro on 5002, stopping one already there, so the run tests the current
code (`HG_METRO_CLEAR=1` also clears its cache), and stops it at the end. Only one run at a time. It opens the installed dev build on Metro, logs in once (skipping the signed-in flows if that fails, rather than
letting each try), then runs the signed-in flows, the protective word flow, the signed-out flows and
the two `resetsApp` flows, in that order, through `scripts/maestro-test.sh`. A failed phase doesn't stop the next. It ends with a
summary, and leaves `metro.log` and a JUnit report per phase in `tests/maestro/out/`. It needs `credentials.env`, the dev
build installed, Development as the app's environment and Device Security off. An Android emulator with a screen
lock must be unlocked: the run wakes it, and waits up to 3 minutes for you to unlock it (until then Android can't open
the app). Every link a flow opens goes through `subflows/open-link.yaml`: iOS 18 and earlier ask "Open in …?" each
time, so for those simulators the run passes `HG_LINK_PROMPT=true` and the step taps Open. The app can start logged in or out:
`start-signed-in` logs in and `start-signed-out` logs out as needed. They reuse the running app instead of relaunching it
for every flow: logged in, they get back to Home by the `healthgateway://home` link, and logged out, to the landing page
by `healthgateway://`. They only ask Maestro questions whose answer is usually "yes", as each "no" costs it up to
7 seconds. The app keeps its login
across launches, so a run logs each account in once. Every flow also ends where the next one starts, and checks it: `subflows/end-signed-in.yaml`
(Home, by the app's `healthgateway://home` link, whether the tabs are a bottom bar or the iPhone Duo's rail) or `subflows/end-signed-out.yaml` (the landing page, by the app's `healthgateway://` link, which
also leaves pages like the VPP hand-off). As a safety net, the start steps relaunch the app when they can't recognise the
screen, such as a menu or dialog a failed flow left open. `subflows/ensure-development.yaml` puts the app back on
Development (logging out and skipping the intro as needed): the test logins only exist there. `maestro-all.sh` runs it
once at the start, inside `subflows/prepare-run.yaml` (one Maestro start that also accepts an iOS "Open in" sheet, waits
for the bundle, and stops the run with an explanation if Device Security is on in the app on that device), and the two `resetsApp` flows run it when they end, pass or fail.

**Protective word:** `records/protective-word.yaml` (tag `protectiveWord`) logs in as a second account whose PharmaNet
medications are protected (`MAESTRO_HG_PROTECTED_USERNAME`, `MAESTRO_HG_PROTECTED_PASSWORD`), enters
`MAESTRO_HG_PROTECTIVE_WORD` in the "Restricted PharmaNet Records" dialog, and logs out. It passes only when a medication
then shows on the timeline (the dialog closing isn't enough: it also hides while a word is checked); on Android a wrong
word is tried first and must be refused. It fails, naming the keys, while they are empty.

**Before running**

1. Start Metro (`npm start`) and open the dev build on a simulator or emulator. Android: `adb reverse tcp:5002 tcp:5002`.
2. Pass the intro once. List device IDs with `xcrun simctl list devices booted` and `adb devices`.
3. Keep your hands off that device while a flow runs, and don't use a phone you need.
4. `signedOut` flows need the app logged out. `signedIn` flows start through `subflows/start-signed-in.yaml`, which logs in
   with the KeyCloak provider from `credentials.env` when the app is on the landing page (run them through
   `scripts/maestro-test.sh`). They need Development, a registered test account with the current terms accepted, and
   Device Security off, as no flow can pass the device prompt. Run `signedIn`, then log out for `signedOut`, and run the two
   `resetsApp` flows last.

**Running**, with `<device>` as the device ID:

```sh
maestro --device <device> test tests/maestro --include-tags=signedIn --exclude-tags=android   # iOS
maestro --device <device> test tests/maestro --include-tags=signedIn                          # Android
maestro --device <device> test tests/maestro --include-tags=signedOut --exclude-tags=resetsApp
maestro --device <device> test tests/maestro/settings/environment.yaml                        # resets the app
maestro --device <device> test tests/maestro/onboarding/onboarding.yaml                       # resets the app
maestro --device <device> test tests/maestro/records                                           # one group
maestro --device <device> test tests/maestro/navigation/logo-home.yaml                        # one flow
pkill -f maestro                                                                               # after every run
```

`pkill` matters on iOS: Maestro's driver otherwise keeps blocking manual input. Results and failure screenshots are in
`~/.maestro/tests/`, newest folder last; screenshots the flows take are in `tests/maestro/out/`.

**Android only** (tagged `android`): `connectivity/offline` (Maestro can only change airplane mode there) and
`connectivity/pull-to-refresh` (on iOS it did not catch the 1.2 second pill).

**Test login (`credentials.env`):** copy `credentials.env.example` to `credentials.env` (git-ignored) and fill in a BC
Services Card test login for Development. Run flows through `scripts/maestro-test.sh` instead of `maestro`, with the same
arguments, from `ClientApp/`, for example `scripts/maestro-test.sh --device <device> test tests/maestro --include-tags=signedIn`. It
hands the `MAESTRO_*` values to the flows through the environment and prints `********` in place of the passwords and
the protective word (usernames are not secret, so they show). Maestro's own run
folders under `~/.maestro/tests/` can still hold what was typed, so don't share them.

**Maestro Studio:** open the `tests/maestro` folder itself, so the flows' `../subflows/` paths resolve. Studio does not
read `credentials.env`, so flows that log in themselves only run from the script.

## Project layout

```
src/app/          Expo Router routes (thin files)  –  src/views/   screens
src/components/   ui primitives + site shell        –  src/services/ HTTP, config, auth (web/native), API services
src/stores/       zustand stores                    –  src/navigation/ user-state guard, route meta, menu
src/theme/        design tokens (also feeds tailwind.config.js) – assets/ fonts and images
```

## Scripts

`npm run typecheck`, `npm run lint` (fix + format), `npm run lintcheck` (CI, zero warnings), `npm run export:web` (static bundle in `dist/`, using `.env`).

`npm run build` is the deployed web build, the contract the Health Gateway WebClient publish relies on (its csproj runs
`npm ci` then `npm run build` and ships `dist/`, as for the Vue ClientApp). It typechecks, lints the app with zero
warnings, then exports `dist/` with `/configuration` from the hosting server and Snowplow on (its own environment can
override either; `.env` cannot), and drops source maps and the native-only `.ttf` fonts.
