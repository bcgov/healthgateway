const CompressionPlugin = require("compression-webpack-plugin");
module.exports = {
    productionSourceMap: false,
    integrity: true,
    lintOnSave: true,
    devServer: {
        overlay: {
            warnings: true,
            errors: true,
        },
    },
    chainWebpack: (config) => {
        config.plugins.delete("split-manifest").delete("inline-manifest");
        config.plugin("CompressionPlugin").use(CompressionPlugin);
        config.resolve.symlinks(false);
    },
};
