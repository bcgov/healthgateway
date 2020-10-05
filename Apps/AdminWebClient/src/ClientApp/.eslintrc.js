module.exports = {
    root: true,
    env: {
        node: true
    },
    extends: [
        "plugin:vue/recommended",
        "plugin:prettier-vue/recommended",
        "prettier/vue",
        "@vue/typescript/recommended",
        "@vue/prettier",
        "@vue/prettier/@typescript-eslint"
    ],
    rules: {
        "no-console": "off",
        "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
        "linebreak-style": ["error", "unix"],
        "vue/valid-v-slot": "off"
    },
    parserOptions: {
        parser: "@typescript-eslint/parser"
    },
    plugins: ["sonarjs", "prettier"]
};
