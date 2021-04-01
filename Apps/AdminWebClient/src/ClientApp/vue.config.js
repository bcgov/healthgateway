module.exports = {
    transpileDependencies: ["vuetify"],
    publicPath: process.env.NODE_ENV === "production" ? "/admin/" : "/",
    productionSourceMap: false,
    lintOnSave: true,
    integrity: true,
    devServer: {
        overlay: {
            warnings: true,
            errors: true
        }
    },
    chainWebpack: config => {
        config.plugins.delete("split-manifest").delete("inline-manifest");
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
        config.plugin("preload").tap(options => {
            options[0].include = "allChunks";
            return options;
        });
    }
};
