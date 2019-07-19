module.exports = {
  verbose: true,
  name: 'Client app',
  displayName: 'Client app',
  moduleFileExtensions: [
    'js',
    'ts',
    'vue'
  ],
  testResultsProcessor: 'jest-sonar-reporter',
  transform: {
    '^.+\\.js$': '<rootDir>/node_modules/babel-jest',
    '^.+\\.(js|jsx|ts|tsx)$': 'ts-jest',
    "^.+\\.vue$": "vue-jest",
    ".+\\.(css|styl|less|sass|scss|svg|png|jpg|ttf|woff|woff2)$": "jest-transform-stub",
  },
  moduleNameMapper: {
    "@/(.*)$": "<rootDir>/app/$1",
  },
  transformIgnorePatterns: [
    'node_modules/(?!(react-native|my-project|react-native-button)/)',
  ],
  testRegex: '.*test.ts',
  collectCoverage: true,
  collectCoverageFrom: [
    '**/*.{ts,jsx,vue}',
    '!**/node_modules/**',
    '!**/vendor/**',
  ],
  coverageDirectory: '<rootDir>/../../sonar_reports/jest.out',
  testMatch: null
}
