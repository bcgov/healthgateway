const { getDefaultConfig } = require("expo/metro-config");
const { withNativeWind } = require("nativewind/metro");

const config = getDefaultConfig(__dirname);

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
