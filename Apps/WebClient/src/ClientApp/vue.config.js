/* eslint-disable */
const CompressionPlugin = require("compression-webpack-plugin");
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');

module.exports = {
    productionSourceMap: false,
    lintOnSave: true,
    integrity: true,
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

        /**
         * Disable (or customize) prefetch, see:
         * https://cli.vuejs.org/guide/html-and-static-assets.html#prefetch
         */
        config.plugins.delete("prefetch");

        /**
         * Configure preload to load all chunks
         * NOTE: use `allChunks` instead of `all` (deprecated)
         */
        config.plugin("preload").tap((options) => {
            options[0].include = "allChunks";
            return options;
        });

        config.plugin('fork-ts-checker')
            .tap(args => {
                let allowUseMem=1024;
                args[0].memoryLimit = allowUseMem;
                return args
            });
    },
};
