// react-native-svg's web build asks for React Native's asset registry, which RN 0.88 no longer ships as a package.
module.exports = { registerAsset: () => 0, getAssetByID: () => undefined };
