module.exports = {

  name: 'Client app',
  displayName: 'Client app',
  rootDir: '../.',
  roots: [
    '<rootDir>/src/', 
    '<rootDir>/tests/'
  ],
  moduleFileExtensions: [
    'js',
    'ts',
  ],
  testResultsProcessor: 'jest-sonar-reporter',
  transform: {
    '^.+\\.js$': '<rootDir>/node_modules/babel-jest',
    '^.+\\.(js|jsx|ts|tsx)$': 'ts-jest',
  },
  transformIgnorePatterns: [
    'node_modules/(?!(react-native|my-project|react-native-button)/)',
  ],
  testRegex: './*.test.ts',
  collectCoverage: true,
  collectCoverageFrom: [
    '**/*.{js,jsx}',
    '!**/node_modules/**',
    '!**/vendor/**',
    '../src/ClientApp/**',
    '../src/**/*.{ts, tsx, js,jsx}',
    '/home/dev/Development/HealthGateway/Apps/WebClient/src/**/*.ts'
  ],
  coverageDirectory: '<rootDir>',
  testMatch: null,
  verbose: true
}
