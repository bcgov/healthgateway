module.exports = {
    root: true,
    env: {
        node: true,
    },
    extends: [
        "plugin:vue/recommended",
        "plugin:prettier-vue/recommended",
        "prettier",
        "@vue/typescript/recommended",
        "@vue/prettier",
        "plugin:sonarjs/recommended",
    ],
    rules: {
        "no-console": "off",
        "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
        "linebreak-style": ["error", "unix"],
        "vue/valid-v-slot": "off",
        "simple-import-sort/imports": "error",
        "simple-import-sort/exports": "error",
    },
    parser: "vue-eslint-parser",
    parserOptions: {
        parser: "@typescript-eslint/parser",
    },
    plugins: ["sonarjs", "prettier", "simple-import-sort"],
};
