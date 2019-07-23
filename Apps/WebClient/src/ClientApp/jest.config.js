module.exports = {
  verbose: true,
  name: 'Client app',
  displayName: 'Client app',
  moduleFileExtensions: [
    "js",
    "jsx",
    "json",
    "vue",
    "ts",
    "tsx"
  ],
  testResultsProcessor: 'jest-sonar-reporter',
  transform: {
    '^.+\\.js$': '<rootDir>/node_modules/babel-jest',
    "^.+\\.tsx?$": "ts-jest",
    "^.+\\.vue$": "vue-jest",
    ".+\\.(css|styl|less|sass|scss|svg|png|jpg|ttf|woff|woff2)$": "jest-transform-stub",
  },
  moduleNameMapper: {
    "@/(.*)$": "<rootDir>/app/$1",
  },
  transformIgnorePatterns: [
    "/node_modules/"
  ],
  collectCoverage: true,
  collectCoverageFrom: [
    '**/*.{ts,jsx,vue}',
    '!**/node_modules/**',
    '!**/vendor/**',
  ],
  coverageDirectory: '<rootDir>/../../sonar_reports/jest.out',
  testMatch: [
    "**/test/**/*.test.(js|jsx|ts|tsx)|**/__tests__/*.(js|jsx|ts|tsx)"
  ]
}
