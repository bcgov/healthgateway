const CompressionPlugin = require("compression-webpack-plugin");
module.exports = {
  productionSourceMap: false,
  configureWebpack: {
    output: {
      libraryExport: "default",
    },
  },
  chainWebpack: (config) => {
    config.plugins.delete("split-manifest").delete("inline-manifest");
    config.plugin("CompressionPlugin").use(CompressionPlugin);
  },
};
