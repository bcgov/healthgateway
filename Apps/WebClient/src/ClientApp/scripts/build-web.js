/* global __dirname */
// npm run build: the deployed web bundle in dist/. The Health Gateway WebClient publish (WebClient.csproj target
// PublishRunWebpack) runs `npm ci` then `npm run build` in its SPA folder and ships dist/, so this keeps its build
// steps unchanged. Plain Node rather than inline VAR=value scripts, so it also runs from Visual Studio on Windows.
const { spawnSync } = require("child_process");
const fs = require("fs");
const path = require("path");

const appRoot = path.join(__dirname, "..");
const outDir = path.join(appRoot, "dist");

/**
 * The export's environment: deployed values unless the caller's environment sets them. Expo lets the process
 * environment win over .env, so a local .env (localhost /configuration, Snowplow off) cannot leak into the bundle.
 */
function buildEnv(env) {
    return {
        ...env,
        // The server that hosts the bundle answers /configuration.
        EXPO_PUBLIC_WEB_CLIENT_BASE_URI:
            env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI ?? "/",
        EXPO_PUBLIC_SNOWPLOW_ENABLED:
            env.EXPO_PUBLIC_SNOWPLOW_ENABLED ?? "true",
        CI: "1",
        EXPO_NO_TELEMETRY: "1",
    };
}

/** Removes what a deployed site should not serve: source maps, and .ttf fonts (web uses woff2). */
function removeUnservedFiles(dir) {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const file = path.join(dir, entry.name);
        if (entry.isDirectory()) removeUnservedFiles(file);
        else if (/\.(map|ttf)$/i.test(entry.name)) fs.rmSync(file);
    }
}

function main() {
    // --clear: web bundles inline the version, and Metro's cache would keep the previous one.
    const result = spawnSync(
        process.execPath,
        [
            require.resolve("expo/bin/cli"),
            "export",
            "--platform",
            "web",
            "--clear",
            "--output-dir",
            outDir,
        ],
        { cwd: appRoot, env: buildEnv(process.env), stdio: "inherit" }
    );
    if (result.status !== 0) process.exit(result.status ?? 1);
    removeUnservedFiles(outDir);
}

if (require.main === module) main();

module.exports = { buildEnv, removeUnservedFiles };
