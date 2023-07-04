module.exports = {
    root: true,
    env: {
        node: true,
    },
    extends: [
        "plugin:vue/vue3-recommended",
        "eslint:recommended",
        "@vue/eslint-config-typescript",
        "@vue/eslint-config-prettier",
    ],
    plugins: ["unused-imports", "simple-import-sort"],
    rules: {
        "@typescript-eslint/no-unused-vars": "off",
        "unused-imports/no-unused-imports": "warn",
        "unused-imports/no-unused-vars": [
            "warn",
            { vars: "all", args: "after-used" },
        ],
        "simple-import-sort/imports": "error",
        "simple-import-sort/exports": "error",
        "vue/multi-word-component-names": "off",
    },
};
