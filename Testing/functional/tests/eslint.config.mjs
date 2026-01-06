import { defineConfig } from "eslint/config";
import pluginCypress from "eslint-plugin-cypress";
import pluginChaiFriendly from "eslint-plugin-chai-friendly";
import eslintConfigPrettier from "eslint-config-prettier/flat";

export default defineConfig(
    {
        files: ["cypress/**/*.js"],
        extends: [
            pluginCypress.configs.globals,
            pluginChaiFriendly.configs.recommendedFlat,
        ],
    },
    eslintConfigPrettier
);
