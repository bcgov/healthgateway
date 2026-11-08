#!/usr/bin/env bash
# Runs every Maestro flow on one platform, start to finish (from ClientApp/: npm run e2e:ios, or npm run e2e:android):
#   scripts/maestro-all.sh ios|android [device id]
# 1. Device: a simulator name ("iPhone Duo") or id, or an emulator serial, as the second argument (a shut-down
#    simulator is booted);
#    HG_ANDROID_AVD names an emulator by AVD (used if running, started if not); otherwise the booted simulator or
#    running emulator, or one started (HG_IOS_SIMULATOR, default "iPhone 16 Pro"; the first AVD listed).
# 2. Starts a fresh Metro on port 5002 (stopping any already there, so the run tests the current code; HG_METRO_CLEAR=1
#    also clears its cache) and stops it at the end. Only one run at a time.
# 3. Opens the installed dev build (ca.bc.gov.healthgateway.dev) on Metro and waits for the bundle.
# 4. Runs, through scripts/maestro-test.sh (logins from credentials.env, values masked): the signedIn flows, the
#    protective word flow (a second account), the signedOut flows, then the two flows that reset the app. A failed
#    phase does not stop the next one.
# Needs credentials.env, the dev build installed, Development as the app's environment and Device Security off. An
# Android emulator with a screen lock must be unlocked: the run wakes it and waits for you.
# Output: tests/maestro/out/ (metro.log, a JUnit report per phase, the flows' screenshots).
set -uo pipefail

platform="${1:-}"
device="${2:-}"
if [[ "$platform" != "ios" && "$platform" != "android" ]]; then
    echo "Usage: scripts/maestro-all.sh ios|android [device id]" >&2
    exit 2
fi

cd "$(dirname "$0")/.."
port=5002
out=tests/maestro/out
mkdir -p "$out"
app_id=ca.bc.gov.healthgateway.dev
android_sdk="${ANDROID_HOME:-$HOME/Library/Android/sdk}"

maestro_env=() # extra -e values for every flow (none secret)
say() { printf '\n=== %s\n' "$*"; }
fail() {
    echo "$*" >&2
    exit 1
}

# one run at a time: two would drive the same device and share Metro's port
lock="$out/.run.lock"
if ! mkdir "$lock" 2>/dev/null; then
    other=$(cat "$lock/pid" 2>/dev/null)
    if [[ -n "$other" ]] && kill -0 "$other" 2>/dev/null; then
        fail "Another run is in progress (pid $other)."
    fi
    rm -rf "$lock"
    mkdir "$lock"
fi
echo $$ >"$lock/pid"

[[ -f credentials.env ]] || fail "No credentials.env: copy credentials.env.example to credentials.env and fill it in."
command -v maestro >/dev/null || fail "maestro is not installed (brew install maestro)."

# ---------------------------------------------------------------- device
if [[ "$platform" == "ios" ]]; then
    # a simulator given by name ("iPhone Duo"): the booted one with that name, otherwise the one with the newest dev
    # build (two simulators can share a name)
    if [[ -n "$device" && ! "$device" =~ ^[0-9A-Fa-f-]{36}$ ]]; then
        name="$device"
        ids=$(xcrun simctl list devices available | grep -F "    $name (" | sed -n 's/.*(\([0-9A-F-]\{36\}\)).*/\1/p')
        [[ -n "$ids" ]] || fail "No simulator named \"$name\" (xcrun simctl list devices available)."
        device=""
        for id in $ids; do
            xcrun simctl list devices booted | grep -q "$id" && device="$id" && break
        done
        if [[ -z "$device" ]]; then
            newest=0
            for id in $ids; do
                build=$(find "$HOME/Library/Developer/CoreSimulator/Devices/$id/data/Containers/Bundle/Application" \
                    -maxdepth 2 -name "HealthGatewayDev.app" 2>/dev/null | head -1)
                [[ -n "$build" ]] || continue
                stamp=$(stat -f %m "$build")
                ((stamp > newest)) && newest=$stamp && device="$id"
            done
        fi
        [[ -n "$device" ]] || device=$(echo "$ids" | head -1)
        echo "Simulator \"$name\": $device"
    fi
    # a simulator named by its id is booted if it isn't already
    if [[ -n "$device" ]] && ! xcrun simctl list devices booted | grep -q "$device"; then
        xcrun simctl list devices | grep -q "$device" || fail "No simulator with id $device."
        say "Booting $device"
        xcrun simctl boot "$device"
        xcrun simctl bootstatus "$device" -b >/dev/null
    fi
    if [[ -z "$device" ]]; then
        device=$(xcrun simctl list devices booted | sed -n 's/.*(\([0-9A-F-]\{36\}\)) (Booted).*/\1/p' | head -1)
    fi
    if [[ -z "$device" ]]; then
        name="${HG_IOS_SIMULATOR:-iPhone 16 Pro}"
        device=$(xcrun simctl list devices available | grep -F "    $name (" | sed -n 's/.*(\([0-9A-F-]\{36\}\)).*/\1/p' | head -1)
        [[ -n "$device" ]] || fail "No booted simulator and none named \"$name\" (set HG_IOS_SIMULATOR)."
        say "Booting $name"
        xcrun simctl boot "$device"
        xcrun simctl bootstatus "$device" -b >/dev/null
    fi
    # show it, so the run can be watched: newer Xcode calls the Simulator app Device Hub
    developer_dir="$(xcode-select -p)"
    open -a Simulator 2>/dev/null ||
        open "$developer_dir/../Applications/DeviceHub.app" 2>/dev/null ||
        open "$developer_dir/Applications/Simulator.app" 2>/dev/null ||
        echo "(could not open the simulator window: the run carries on without it)"
    xcrun simctl get_app_container "$device" "$app_id" >/dev/null 2>&1 ||
        fail "The dev build ($app_id) is not installed on $device: run npm run ios once."
    # iOS 18 and earlier ask "Open in …?" for every link a flow opens; the flows answer it when told to
    ios_version=$(xcrun simctl list devices | awk -v id="$device" '/^-- / { version = $3 } index($0, id) { print version; exit }')
    if [[ -n "$ios_version" && "${ios_version%%.*}" -lt 26 ]]; then
        maestro_env+=(-e HG_LINK_PROMPT=true)
        echo "iOS $ios_version: the flows will answer its \"Open in\" prompt for each link"
    fi
else
    adb() { "$android_sdk/platform-tools/adb" "$@"; }
    # HG_ANDROID_AVD names the emulator: a running one with that AVD is used, otherwise it is started
    if [[ -z "$device" && -n "${HG_ANDROID_AVD:-}" ]]; then
        for serial in $(adb devices | awk 'NR > 1 && $2 == "device" { print $1 }'); do
            [[ "$(adb -s "$serial" emu avd name 2>/dev/null | head -1 | tr -d '\r')" == "$HG_ANDROID_AVD" ]] && device="$serial"
        done
    elif [[ -z "$device" ]]; then
        device=$(adb devices | awk 'NR > 1 && $2 == "device" { print $1; exit }')
    fi
    if [[ -z "$device" ]]; then
        avd="${HG_ANDROID_AVD:-$("$android_sdk/emulator/emulator" -list-avds | head -1)}"
        [[ -n "$avd" ]] || fail "No running emulator and no AVD to start (set HG_ANDROID_AVD)."
        say "Starting emulator $avd"
        nohup "$android_sdk/emulator/emulator" -avd "$avd" -no-snapshot-save -no-boot-anim >"$out/emulator.log" 2>&1 &
        # the new emulator's serial: wait until one reports this AVD
        for _ in $(seq 1 90); do
            for serial in $(adb devices | awk 'NR > 1 { print $1 }'); do
                [[ "$(adb -s "$serial" emu avd name 2>/dev/null | head -1 | tr -d '\r')" == "$avd" ]] && device="$serial"
            done
            [[ -n "$device" ]] && break
            sleep 2
        done
        [[ -n "$device" ]] || fail "Emulator $avd did not start: see $out/emulator.log"
        adb -s "$device" wait-for-device
        for _ in $(seq 1 120); do
            [[ "$(adb -s "$device" shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" == 1 ]] && break
            sleep 2
        done
    fi
    if ! adb -s "$device" shell pm path "$app_id" >/dev/null 2>&1; then
        apk=android/app/build/outputs/apk/debug/app-debug.apk
        [[ -f "$apk" ]] || fail "The dev build ($app_id) is not installed on $device: run npm run android once."
        say "Installing the dev build on $device ($apk)"
        adb -s "$device" install -r "$apk" >/dev/null || fail "Could not install $apk on $device."
    fi
    adb -s "$device" reverse "tcp:$port" "tcp:$port" >/dev/null
    # Wake the screen and lift a lock screen without a PIN. A PIN or password needs you: until the emulator is unlocked
    # (and, after a start, unlocked once) Android can't open the app, and the run would wait on a black screen.
    adb -s "$device" shell input keyevent KEYCODE_WAKEUP
    adb -s "$device" shell wm dismiss-keyguard
    sleep 1
    locked() {
        adb -s "$device" shell dumpsys activity activities | grep -q "mKeyguardShowing=true" ||
            adb -s "$device" shell dumpsys user | grep -q "RUNNING_LOCKED"
    }
    if locked; then
        say "The emulator is locked: unlock it in its window (waiting up to 3 minutes)"
        echo "To skip this next time, remove its screen lock: Settings, Security & privacy, Device unlock, Screen lock, None."
        for _ in $(seq 1 90); do
            locked || break
            sleep 2
        done
        locked && fail "The emulator is still locked: unlock it and run again."
    fi
fi
say "Device: $device ($platform)"

# ---------------------------------------------------------------- Metro
metro_pid=""
cleanup() {
    pkill -f maestro.cli.AppKt 2>/dev/null
    rm -rf "$lock"
    if [[ -n "$metro_pid" ]]; then
        echo "Stopping Metro"
        kill "$metro_pid" 2>/dev/null
        wait "$metro_pid" 2>/dev/null
    fi
}
trap cleanup EXIT

# Always a fresh Metro, so the run tests the current code and .env: a Metro already on the port is stopped first.
running=$(lsof -t -iTCP:"$port" -sTCP:LISTEN 2>/dev/null)
if [[ -n "$running" ]]; then
    ps -o command= -p $running | grep -q -E "expo|metro" ||
        fail "Port $port is in use by something other than Metro: stop it and run again."
    say "Stopping the Metro already on $port, to start a fresh one"
    kill $running 2>/dev/null
    for _ in $(seq 1 15); do
        lsof -nP -iTCP:"$port" -sTCP:LISTEN >/dev/null 2>&1 || break
        sleep 1
    done
    lsof -nP -iTCP:"$port" -sTCP:LISTEN >/dev/null 2>&1 && fail "Port $port is still in use: stop what holds it and run again."
fi
clear_cache=()
[[ "${HG_METRO_CLEAR:-}" == 1 ]] && clear_cache=(--clear)
say "Starting Metro on $port (log: $out/metro.log)"
node_modules/.bin/expo start --port "$port" --dev-client "${clear_cache[@]}" >"$out/metro.log" 2>&1 &
metro_pid=$!
for _ in $(seq 1 90); do
    curl -s "http://localhost:$port/status" | grep -q "packager-status:running" && break
    kill -0 "$metro_pid" 2>/dev/null || fail "Metro stopped: see $out/metro.log"
    sleep 2
done
curl -s "http://localhost:$port/status" | grep -q "packager-status:running" || fail "Metro did not start: see $out/metro.log"

# ---------------------------------------------------------------- open the dev build on Metro
maestro_test() { scripts/maestro-test.sh --device "$device" test ${maestro_env[@]+"${maestro_env[@]}"} "$@"; }

say "Opening the dev build on Metro (the first bundle can take a few minutes)"
if [[ "$platform" == "ios" ]]; then
    xcrun simctl terminate "$device" "$app_id" >/dev/null 2>&1
    # a fresh install: no Expo dev-menu introduction or floating gear over the app (AGENTS.md)
    xcrun simctl spawn "$device" defaults write "$app_id" EXDevMenuIsOnboardingFinished -bool YES
    xcrun simctl spawn "$device" defaults write "$app_id" EXDevMenuShowFloatingActionButton -bool NO
    xcrun simctl openurl "$device" "exp+hgui://expo-development-client/?url=http%3A%2F%2Flocalhost%3A$port"
else
    adb -s "$device" shell am force-stop "$app_id"
    adb -s "$device" shell am start -a android.intent.action.VIEW \
        -d "healthgateway://expo-development-client/?url=http%3A%2F%2F10.0.2.2%3A$port" >/dev/null
fi
# one Maestro start: accept an "Open in" sheet, wait for the bundle, and put the app on Development (a run that stopped
# halfway through switching environment can leave it elsewhere, where the test logins don't exist)
maestro_test tests/maestro/subflows/prepare-run.yaml ||
    fail "The app did not load from Metro, could not be put on Development, or has Device Security on (turn it off on the Security page): see the message above and $out/metro.log"

# ---------------------------------------------------------------- flows
exclude=""
[[ "$platform" == "ios" ]] && exclude="android" # Android-only flows: offline, pull-to-refresh

results=()
status=0
phase() {
    local name="$1" report="$2"
    shift 2
    say "$name"
    if maestro_test "$@" --format junit --output "$out/report-$platform-$report.xml"; then
        results+=("passed  $name")
    else
        results+=("FAILED  $name")
        status=1
    fi
}

# log in once before the signed-in flows (each one would otherwise try, and fail, on its own)
say "Log in"
if maestro_test tests/maestro/subflows/start-signed-in.yaml; then
    phase "Signed in" signed-in tests/maestro --include-tags=signedIn ${exclude:+--exclude-tags=$exclude}
else
    results+=("FAILED  Log in: the signed-in flows were skipped")
    status=1
fi
phase "Protective word (second account)" protective-word tests/maestro --include-tags=protectiveWord
phase "Signed out" signed-out tests/maestro --include-tags=signedOut --exclude-tags="resetsApp${exclude:+,$exclude}"
phase "Switch environment (resets the app)" environment tests/maestro/settings/environment.yaml
phase "Onboarding (resets the app)" onboarding tests/maestro/onboarding/onboarding.yaml

say "Summary ($platform, $device)"
printf '  %s\n' "${results[@]}"
echo "  Reports: $out/report-$platform-*.xml; failure screenshots: ~/.maestro/tests/ (newest last)"
exit "$status"
