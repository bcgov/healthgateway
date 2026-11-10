/* global __dirname, __filename */
// The dev app on an Android emulator or an iOS simulator for local development, run by the Aspire host
// (Apps/AspireHost: the MobileApp and MobileBuild resources) or by hand. Both commands use Metro on port 5002
// (`npm run dev`, the Aspire "ClientApp" resource).
//   node scripts/mobile-dev.js open    opens the installed dev app on Metro
//   node scripts/mobile-dev.js build   builds and installs the dev app (after native changes), then opens it
// HG_MOBILE_PLATFORM: android (the default) or ios (macOS only).
// HG_MOBILE_DEVICE: the emulator or simulator, by name, e.g. Pixel_10_Pro_XL (an AVD, as `emulator -list-avds` gives it)
// or "iPhone 18 Pro Max" (a simulator, as `xcrun simctl list devices` gives it; a UDID also works). Without one: a running
// one, else the first AVD, or the newest iOS's first iPhone. One that isn't running is started: run by the Aspire host
// (HG_ASPIRE_PID), it is shut down when Aspire stops; run by hand, it is left running.
// Android: the SDK comes from ANDROID_HOME (or ANDROID_SDK_ROOT), else its default place; building needs JAVA_HOME.
// `adb reverse` makes the computer's localhost the emulator's for Metro, the WebClient's /configuration, and each
// localhost service endpoint that configuration lists, so the app's Local environment works unchanged; it needs the
// WebClient running. iOS: Xcode and its simulators (which share the computer's localhost).
const { execFileSync, spawn, spawnSync } = require("child_process");
const fs = require("fs");
const os = require("os");
const path = require("path");

const appDir = path.join(__dirname, "..");
const METRO_PORT = 5002;
// the Local environment's configuration (src/config/environments.ts), served by the .NET WebClient
const CONFIG_URL = "http://localhost:3025/configuration";
const APP_ID = "ca.bc.gov.healthgateway.dev"; // the dev build's Android package and iOS bundle ID
// The dev app's own link: open the app on this Metro (Android: adb reverse makes the computer's localhost the emulator's)
const DEV_APP_URL = `exp+hgui://expo-development-client/?url=${encodeURIComponent(`http://localhost:${METRO_PORT}`)}`;
const isWindows = process.platform === "win32";

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

async function waitFor(what, check, seconds) {
    for (let waited = 0; waited < seconds; waited += 2) {
        const result = check();
        if (result) return result;
        await sleep(2000);
    }
    throw new Error(`Gave up waiting ${seconds} s for ${what}`);
}

/** Metro answers on its port (Aspire may report it running before it listens). */
async function waitForMetro() {
    for (let waited = 0; waited < 180; waited += 2) {
        try {
            const response = await fetch(
                `http://localhost:${METRO_PORT}/status`
            );
            if ((await response.text()).includes("packager-status:running"))
                return;
        } catch {
            // not listening yet
        }
        await sleep(2000);
    }
    throw new Error(
        `Metro is not running on port ${METRO_PORT}: start the ClientApp resource, or npm run dev`
    );
}

const LOCAL_HOSTNAMES = new Set(["localhost", "127.0.0.1", "[::1]"]);

/** The port of a URL on this computer, else undefined (a remote endpoint needs no forward). */
function localPort(value) {
    let url;
    try {
        url = new URL(value);
    } catch {
        return undefined;
    }
    if (!LOCAL_HOSTNAMES.has(url.hostname)) return undefined;
    return Number(url.port || (url.protocol === "https:" ? 443 : 80));
}

/**
 * The ports the app reaches on this computer: Metro, the configuration's, and those of the configuration's
 * `serviceEndpoints`, once each (several services can share an endpoint, e.g. Patient and PatientData).
 */
async function localServicePorts() {
    let config;
    try {
        const response = await fetch(CONFIG_URL);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        config = await response.json();
    } catch (error) {
        throw new Error(
            `The configuration at ${CONFIG_URL} is unavailable (${error.message}): start the WebClient resource`
        );
    }
    const endpoints = Object.values(config?.serviceEndpoints ?? {});
    const ports = new Set([METRO_PORT, localPort(CONFIG_URL)]);
    for (const endpoint of endpoints) {
        const port = localPort(endpoint);
        if (port !== undefined) ports.add(port);
    }
    return [...ports];
}

/** Builds with Expo for one device. --no-bundler: Metro is already running (no second copy on 5002). */
function expoRun(platform, device) {
    const cli = require.resolve("expo/bin/cli", { paths: [appDir] });
    const result = spawnSync(
        process.execPath,
        [cli, `run:${platform}`, "--no-bundler", "--device", device],
        { cwd: appDir, stdio: "inherit" }
    );
    if (result.status !== 0)
        throw new Error(
            `expo run:${platform} failed (exit code ${result.status})`
        );
}

// ---- Android ----

function sdkRoot() {
    const fromEnv = process.env.ANDROID_HOME || process.env.ANDROID_SDK_ROOT;
    if (fromEnv) return fromEnv;
    if (isWindows)
        return path.join(process.env.LOCALAPPDATA ?? "", "Android", "Sdk");
    return path.join(
        os.homedir(),
        process.platform === "darwin" ? "Library/Android/sdk" : "Android/Sdk"
    );
}

function sdkTool(folder, name) {
    const file = path.join(sdkRoot(), folder, isWindows ? `${name}.exe` : name);
    if (!fs.existsSync(file))
        throw new Error(
            `${file} not found: install the Android SDK (Android Studio), or set ANDROID_HOME to it`
        );
    return file;
}

const adb = (...args) =>
    execFileSync(sdkTool("platform-tools", "adb"), args, { encoding: "utf8" });

/** The running emulators, each with the AVD it runs. */
function runningEmulators() {
    return adb("devices")
        .split(/\r?\n/)
        .map((line) => line.trim().split(/\s+/))
        .filter(
            ([serial, state]) =>
                serial?.startsWith("emulator-") && state === "device"
        )
        .map(([serial]) => ({
            serial,
            avd: adb("-s", serial, "emu", "avd", "name")
                .split(/\r?\n/)[0]
                .trim(),
        }));
}

const android = {
    /** A running emulator for the wanted AVD (any, without one), started when there is none. */
    async device(wanted) {
        const running = runningEmulators();
        const match = wanted
            ? running.find((e) => e.avd === wanted)
            : running[0];
        if (match) {
            console.log(
                `Using the running emulator ${match.avd} (${match.serial})`
            );
            return { id: match.serial, name: match.avd, started: false };
        }
        const emulator = sdkTool("emulator", "emulator");
        const avds = execFileSync(emulator, ["-list-avds"], {
            encoding: "utf8",
        })
            .split(/\r?\n/)
            .map((name) => name.trim())
            .filter(Boolean);
        const avd = wanted || avds[0];
        if (!avd)
            throw new Error(
                "No Android Virtual Device: create one in Android Studio (Device Manager)"
            );
        if (!avds.includes(avd))
            throw new Error(
                `No AVD named "${avd}": the AVDs are ${avds.join(", ")}`
            );
        console.log(`Starting the emulator ${avd}`);
        // detached: it stays up after this script (shutDownWithAspire stops it with Aspire)
        spawn(emulator, ["-avd", avd], {
            detached: true,
            stdio: "ignore",
        }).unref();
        const serial = await waitFor(
            `the emulator ${avd} to start`,
            () => runningEmulators().find((e) => e.avd === avd)?.serial,
            180
        );
        await waitFor(
            "Android to finish booting",
            () =>
                adb(
                    "-s",
                    serial,
                    "shell",
                    "getprop",
                    "sys.boot_completed"
                ).trim() === "1",
            180
        );
        return { id: serial, name: avd, started: true };
    },

    build(device) {
        expoRun("android", device.name);
    },

    shutdown(id) {
        adb("-s", id, "emu", "kill");
    },

    async open(device) {
        const ports = await localServicePorts();
        for (const port of ports)
            adb("-s", device.id, "reverse", `tcp:${port}`, `tcp:${port}`);
        console.log(
            `Forwarded the emulator's localhost ports ${ports.join(", ")} to this computer`
        );
        const installed = adb(
            "-s",
            device.id,
            "shell",
            "pm",
            "list",
            "packages",
            APP_ID
        )
            .split(/\r?\n/)
            .some((line) => line.trim() === `package:${APP_ID}`);
        if (!installed) throw notInstalled(device);
        // stopped first: after an install, Expo opens it on its default Metro port, and a link to a running app can end
        // up behind that error screen
        adb("-s", device.id, "shell", "am", "force-stop", APP_ID);
        // one argument for the device's shell, quoted so it is taken as it is
        adb(
            "-s",
            device.id,
            "shell",
            `am start -a android.intent.action.VIEW -d '${DEV_APP_URL}' ${APP_ID}`
        );
    },
};

// ---- iOS ----

// its error output stays in the thrown error: the messages here say what went wrong
const simctl = (...args) =>
    execFileSync("xcrun", ["simctl", ...args], {
        encoding: "utf8",
        stdio: ["ignore", "pipe", "pipe"],
    });

/** The available iPhone and iPad simulators, the newest iOS first. */
function simulators() {
    const { devices } = JSON.parse(
        simctl("list", "devices", "available", "--json")
    );
    const version = (runtime) =>
        (/iOS-(\d+)-(\d+)/.exec(runtime) ?? [0, 0, 0]).slice(1).map(Number);
    return Object.entries(devices)
        .filter(([runtime]) => runtime.includes("iOS"))
        .sort(([a], [b]) => {
            const [am, an] = version(a);
            const [bm, bn] = version(b);
            return bm - am || bn - an;
        })
        .flatMap(([runtime, list]) =>
            list.map((d) => ({
                udid: d.udid,
                name: d.name,
                state: d.state,
                runtime,
            }))
        );
}

/**
 * Shows the simulators: Device Hub from Xcode 27 (by bundle ID, wherever Xcode is), else the Simulator app of an older
 * Xcode, on this simulator. A simulator runs without either; this only shows it.
 */
function showSimulator(udid) {
    const quietly = { stdio: "ignore" };
    try {
        execFileSync("open", ["-b", "com.apple.dt.Devices"], quietly);
        return;
    } catch {
        // an Xcode before 27
    }
    try {
        execFileSync(
            "open",
            ["-a", "Simulator", "--args", "-CurrentDeviceUDID", udid],
            quietly
        );
    } catch {
        console.log(
            "Neither Device Hub nor Simulator to show it in: it runs without a window"
        );
    }
}

const ios = {
    /** The wanted simulator (by name or UDID; a booted one, or the newest iOS's), booted when it isn't. */
    async device(wanted) {
        if (process.platform !== "darwin")
            throw new Error(
                "iOS simulators need macOS: set HG_MOBILE_PLATFORM to android"
            );
        const all = simulators();
        const candidates = wanted
            ? all.filter((s) => s.name === wanted || s.udid === wanted)
            : all.filter((s) => s.name.startsWith("iPhone"));
        const sim =
            candidates.find((s) => s.state === "Booted") ?? candidates[0];
        if (!sim)
            throw new Error(
                wanted
                    ? `No simulator named "${wanted}": see xcrun simctl list devices available`
                    : "No iPhone simulator: add one in Xcode (Window > Devices and Simulators)"
            );
        if (sim.state === "Booted") {
            console.log(`Using the booted simulator ${sim.name} (${sim.udid})`);
        } else {
            console.log(
                `Starting the simulator ${sim.name} (${sim.runtime.split(".").pop()})`
            );
            simctl("boot", sim.udid);
        }
        showSimulator(sim.udid);
        // until the simulator has finished booting
        simctl("bootstatus", sim.udid, "-b");
        return {
            id: sim.udid,
            name: sim.name,
            started: sim.state !== "Booted",
        };
    },

    build(device) {
        expoRun("ios", device.id);
    },

    shutdown(id) {
        simctl("shutdown", id);
    },

    open(device) {
        try {
            simctl("get_app_container", device.id, APP_ID);
        } catch {
            throw notInstalled(device);
        }
        // stopped first, as on Android: after an install, Expo opens it on its default Metro port
        try {
            simctl("terminate", device.id, APP_ID);
        } catch {
            // not running
        }
        simctl("openurl", device.id, DEV_APP_URL);
    },
};

function notInstalled(device) {
    return new Error(
        `The dev app (${APP_ID}) is not installed on ${device.name}: build it first (the MobileBuild resource, or node scripts/mobile-dev.js build)`
    );
}

function platformFrom(name) {
    const platformName = (name?.trim() || "android").toLowerCase();
    const platform = { android, ios }[platformName];
    if (!platform)
        throw new Error(
            `HG_MOBILE_PLATFORM must be android or ios, not "${platformName}"`
        );
    return platform;
}

const isRunning = (pid) => {
    try {
        process.kill(pid, 0);
        return true;
    } catch (error) {
        // EPERM: it exists, but belongs to someone else
        return error.code === "EPERM";
    }
};

/**
 * Run by the Aspire host (HG_ASPIRE_PID): a device this script started is shut down when the host exits, so stopping
 * Aspire stops everything it started. A small process of its own, left behind detached, so it also works when the host
 * is killed outright (Windows) and when this resource is restarted in the dashboard (the device stays up meanwhile).
 */
function shutDownWithAspire(platformName, device) {
    const aspirePid = Number(process.env.HG_ASPIRE_PID);
    if (!device.started || !Number.isInteger(aspirePid) || aspirePid <= 0)
        return;
    spawn(
        process.execPath,
        [__filename, "watch", platformName, device.id, String(aspirePid)],
        { detached: true, stdio: "ignore" }
    ).unref();
    console.log(`${device.name} will be shut down when Aspire stops`);
}

async function watch(platformName, id, aspirePid) {
    const platform = platformFrom(platformName);
    while (isRunning(Number(aspirePid))) await sleep(3000);
    try {
        platform.shutdown(id);
    } catch {
        // already shut down
    }
}

async function main(command, ...rest) {
    if (command === "watch") return watch(...rest);
    if (command !== "open" && command !== "build")
        throw new Error('Use "open" or "build"');
    const platformName = process.env.HG_MOBILE_PLATFORM;
    const platform = platformFrom(platformName);
    await waitForMetro();
    const device = await platform.device(
        process.env.HG_MOBILE_DEVICE?.trim() || undefined
    );
    shutDownWithAspire(platformName?.trim() || "android", device);
    if (command === "build") platform.build(device);
    await platform.open(device);
    console.log(
        `Opened the dev app on ${device.name}, on Metro at port ${METRO_PORT}`
    );
}

main(...process.argv.slice(2)).catch((error) => {
    console.error(error.message);
    process.exit(1);
});
