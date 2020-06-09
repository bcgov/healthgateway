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
    config.optimization.minimizer("terser").tap((args) => {
      args[0].parallel = false;
      return args;
    });
    // TODO: Only needed while typescrit has errors
    config.plugins.delete("fork-ts-checker");
  },
};
