const CompressionPlugin = require("compression-webpack-plugin");
module.exports = {
    productionSourceMap: false,
    integrity: true,
    chainWebpack: (config) => {
        config.plugins.delete("split-manifest").delete("inline-manifest");
        config.plugin("CompressionPlugin").use(CompressionPlugin);
        config.resolve.symlinks(false);
    },
};
