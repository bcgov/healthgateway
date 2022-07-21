module.exports = {
    transpileDependencies: ["vuetify"],
    publicPath: process.env.NODE_ENV === "production" ? "/admin/" : "/",
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
    chainWebpack: (config) => {
        config.plugins.delete("split-manifest").delete("inline-manifest");
        config.resolve.symlinks(false);

        /**
         * Disable (or customize) prefetch, see:
         * https://cli.vuejs.org/guide/html-and-static-assets.html#prefetch
         */
        config.plugins.delete("prefetch");
    },
};
