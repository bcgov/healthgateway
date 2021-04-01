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
    "@vue/prettier/@typescript-eslint",
  ],
  rules: {
    "no-console": "off",
    "vue/no-v-html": "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
    "linebreak-style": ["error", "unix"],
    "simple-import-sort/imports": "error",
    "simple-import-sort/exports": "error",
  },
  parserOptions: {
    parser: "@typescript-eslint/parser",
  },
  plugins: ["prettier", "simple-import-sort"],
};
