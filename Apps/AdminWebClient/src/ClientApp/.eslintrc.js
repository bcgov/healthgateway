module.exports = {
  root: true,
  env: {
    node: true
  },
  extends: [
    "eslint:recommended",
    "plugin:vue/recommended",
    "@vue/prettier",
    "@vue/typescript",
    "plugin:sonarjs/recommended"
  ],
  rules: {
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off"
  },
  parserOptions: {
    parser: "@typescript-eslint/parser"
  },
  plugins: ["sonarjs"]
};
