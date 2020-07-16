module.exports = {
  verbose: true,
  name: "Client app",
  displayName: "Client app",
  moduleFileExtensions: ["js", "json", "vue", "ts"],
  testResultsProcessor: "jest-sonar-reporter",
  transform: {
    "^.+\\.(js)$": "<rootDir>/node_modules/babel-jest",
    "^.+\\.tsx?$": "ts-jest",
    "^.+\\.vue$": "vue-jest",
    ".+\\.(css|styl|less|sass|scss|svg|png|jpg|ttf|woff|woff2)$":
      "jest-transform-stub",
    "^.+\\.(bmp|gif|jpg|jpeg|png|psd|svg|webp|html)$":
      "<rootDir>/test/mocks/mediaFileTransformer.js",
  },
  moduleNameMapper: {
    "@/(.*)$": "<rootDir>/src/$1",
    "\\.(css)$": "<rootDir>/test/mocks/styleMock.js",
  },
  transformIgnorePatterns: ["/node_modules/"],
  collectCoverage: true,
  collectCoverageFrom: [
    "**/*.{ts,vue}",
    "!**/node_modules/**",
    "!**/vendor/**"
  ],
  coverageDirectory: "<rootDir>/../../sonar_reports/jest.out",
  testMatch: ["**/test/**/*.test.(ts)|**/__tests__/*.(ts)"],
  globals: {
    _NODE_ENV: "development",
  },
};
