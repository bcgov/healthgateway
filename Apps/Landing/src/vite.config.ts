import { fileURLToPath, URL } from "node:url";

import vue from "@vitejs/plugin-vue";
import dotenv from "dotenv";
import { defineConfig } from "vite";
import vuetify, { transformAssetUrls } from "vite-plugin-vuetify";

// Load environment variables from the appropriate .env file
dotenv.config({
    path: fileURLToPath(
        new URL(
            process.env.NODE_ENV === "production"
                ? "./.env.production"
                : "./.env.development",
            import.meta.url
        )
    ),
});

// Prepare environment variables for Vite's define option
const envVariables = Object.keys(process.env).reduce((prev, curr) => {
    prev[`process.env.${curr}`] = JSON.stringify(process.env[curr]);
    return prev;
}, {});

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        vue({
            template: { transformAssetUrls },
        }),
        // https://github.com/vuetifyjs/vuetify-loader/tree/next/packages/vite-plugin
        vuetify({
            autoImport: true,
        }),
    ],
    optimizeDeps: {
        exclude: ["vuetify"],
    },
    define: envVariables, // Inject environment variables
    resolve: {
        alias: {
            "@": fileURLToPath(new URL("./src", import.meta.url)),
        },
        extensions: [".js", ".json", ".jsx", ".mjs", ".ts", ".tsx", ".vue"],
        preserveSymlinks: true,
    },
    server: {
        host: "127.0.0.1",
        port: 5003,
    },
});
