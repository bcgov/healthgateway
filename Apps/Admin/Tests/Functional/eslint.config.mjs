import { defineConfig } from "eslint/config";
import eslintConfigPrettier from "eslint-config-prettier/flat";
import pluginChaiFriendly from "eslint-plugin-chai-friendly";
import pluginCypress from "eslint-plugin-cypress";
import simpleImportSort from "eslint-plugin-simple-import-sort";
import unusedImports from "eslint-plugin-unused-imports";

export default defineConfig(
    {
        files: ["cypress/**/*.js"],
        extends: [
            pluginCypress.configs.globals,
            pluginChaiFriendly.configs.recommendedFlat,
        ],
    },
    {
        plugins: { "unused-imports": unusedImports },
        rules: {
            "unused-imports/no-unused-imports": "warn",
            "unused-imports/no-unused-vars": [
                "warn",
                { vars: "all", args: "after-used", argsIgnorePattern: "^_" },
            ],
        },
    },
    {
        plugins: { "simple-import-sort": simpleImportSort },
        rules: {
            "simple-import-sort/imports": "error",
            "simple-import-sort/exports": "error",
        },
    },
    eslintConfigPrettier
);
