const path = require("path");
const isDevBuild = true;
const bundleOutputDir = "/dist";
module.exports = {
  filenameHashing: false,
  css: {
    extract: false,
  },
  configureWebpack: {
    optimization: {
      splitChunks: false,
    },
  },
  chainWebpack: (config) => {
    config.plugins.delete("split-manifest").delete("inline-manifest");
  },
};
