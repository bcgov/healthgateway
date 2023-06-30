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
    plugins: ["simple-import-sort"],
    rules: {
        "simple-import-sort/imports": "error",
        "simple-import-sort/exports": "error",
        "vue/multi-word-component-names": "off",
    },
};
