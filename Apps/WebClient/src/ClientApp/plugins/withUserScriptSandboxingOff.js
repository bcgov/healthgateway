// Plain CommonJS on purpose: Expo's config loader transpiles app.config.ts itself but resolves the modules it
// imports with Node's require, so a nested .ts file is "not found" from the Xcode build scripts.
const { withXcodeProject } = require("expo/config-plugins");

/**
 * Xcode's User Script Sandboxing (on by default in generated projects) blocks React Native's
 * "Bundle React Native code and images" phase from writing `ip.txt` into the app for device Debug builds,
 * so builds started from Xcode fail with "Operation not permitted". Prebuild regenerates `ios/`, so the
 * setting is applied here rather than edited by hand.
 * @type {import("expo/config-plugins").ConfigPlugin}
 */
const withUserScriptSandboxingOff = (config) =>
    withXcodeProject(config, (c) => {
        const configurations = c.modResults.pbxXCBuildConfigurationSection();
        for (const entry of Object.values(configurations)) {
            if (entry && typeof entry === "object" && entry.buildSettings) {
                entry.buildSettings.ENABLE_USER_SCRIPT_SANDBOXING = "NO";
            }
        }
        return c;
    });

module.exports = withUserScriptSandboxingOff;
