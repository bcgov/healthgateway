/* eslint-disable */
const CompressionPlugin = require("compression-webpack-plugin");
const ForkTsCheckerWebpackPlugin = require("fork-ts-checker-webpack-plugin");
const NodePolyfillPlugin = require("node-polyfill-webpack-plugin");

module.exports = {
    productionSourceMap: false,
    lintOnSave: true,
    integrity:
        process.env.VUE_APP_CONFIG_INTEGRITY === undefined
            ? true
            : process.env.VUE_APP_CONFIG_INTEGRITY === "true",
    devServer: {
        client: {
            logging: "info",
            overlay: true,
            progress: true,
        },
    },
    // Other rules...
    configureWebpack: {
        resolve: {
            fallback: {
                fs: false,
            },
        },
        plugins: [
            new ForkTsCheckerWebpackPlugin({
                typescript: {
                    memoryLimit: 1024,
                },
            }),
            new NodePolyfillPlugin(),
        ],
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
    },
    transpileDependencies: ["vuex-persist"],
};
