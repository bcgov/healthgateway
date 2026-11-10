const crypto = require("crypto");
const fs = require("fs");
const path = require("path");

const { getDefaultConfig } = require("expo/metro-config");
const { withNativeWind } = require("nativewind/metro");

const config = getDefaultConfig(__dirname);

// Web reads the app config (version, extra.build, …) from a copy compiled into expo-constants, and Metro keeps compiled
// files between runs: without this, a changed app.config.ts or version.json shows on web only after `expo start --clear`.
// Part of every cache key, so the cache starts over only when one of them changes.
const appConfigHash = crypto.createHash("sha256");
for (const file of ["app.config.ts", "version.json"])
    appConfigHash.update(fs.readFileSync(path.join(__dirname, file)));
config.cacheVersion = `${config.cacheVersion ?? ""}-app-${appConfigHash.digest("hex").slice(0, 12)}`;

// SVG files are imported as React components (react-native-svg-transformer).
config.transformer = {
    ...config.transformer,
    babelTransformerPath: require.resolve("react-native-svg-transformer/expo"),
};
config.resolver = {
    ...config.resolver,
    assetExts: config.resolver.assetExts.filter((ext) => ext !== "svg"),
    sourceExts: [...config.resolver.sourceExts, "svg"],
};

module.exports = withNativeWind(config, { input: "./src/global.css" });
