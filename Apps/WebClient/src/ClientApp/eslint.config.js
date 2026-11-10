// https://docs.expo.dev/guides/using-eslint/
// Rules aligned with the Vue client's ../ClientApp.old/eslint.config.mjs
const { defineConfig } = require("eslint/config");
const expoConfig = require("eslint-config-expo/flat");
const eslintConfigPrettier = require("eslint-config-prettier/flat");
const simpleImportSort = require("eslint-plugin-simple-import-sort");
const typescriptEslint = require("@typescript-eslint/eslint-plugin");
const unusedImports = require("eslint-plugin-unused-imports");

const typeAwareRules = Object.assign(
    {},
    ...typescriptEslint.configs["flat/recommended-type-checked"].map(
        (config) => config.rules ?? {}
    ),
    { "@typescript-eslint/no-unused-vars": "off" }
);

module.exports = defineConfig([
    expoConfig,
    {
        ignores: ["dist/*", "ios/*", "android/*", ".expo/*", "coverage/*"],
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
            "import/order": "off",
        },
    },
    {
        files: ["**/*.ts", "**/*.tsx"],
        rules: {
            "@typescript-eslint/no-unused-vars": "off",
            "@typescript-eslint/no-explicit-any": "warn",
        },
    },
    {
        // Type-aware, the plugin's recommended set. `void` marks async work deliberately not waited for.
        files: ["**/*.ts", "**/*.tsx"],
        languageOptions: {
            parserOptions: { projectService: true },
        },
        rules: typeAwareRules,
    },
    {
        // async mocks and expect(obj.method) are normal in tests
        files: ["tests/unit/**/*.ts", "tests/unit/**/*.tsx"],
        rules: {
            "@typescript-eslint/require-await": "off",
            "@typescript-eslint/unbound-method": "off",
        },
    },
    eslintConfigPrettier,
]);
