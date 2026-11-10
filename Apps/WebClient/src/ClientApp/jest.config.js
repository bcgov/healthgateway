// Snowplow is on unless switched off, and Jest reads no .env file: keep unit tests from starting a real tracker.
// Set here, before any transform or worker, so it holds whether babel-preset-expo inlines it or the code reads it.
process.env.EXPO_PUBLIC_SNOWPLOW_ENABLED ??= "false";

const shared = {
    setupFiles: ["<rootDir>/jest.setup.js"],
    setupFilesAfterEnv: ["<rootDir>/jest.afterEnv.js"],
    moduleNameMapper: {
        "\\.svg$": "<rootDir>/tests/unit/support/svgMock.js",
        "\\.css$": "<rootDir>/tests/unit/support/cssMock.js",
        "^@react-native/assets-registry/registry$":
            "<rootDir>/tests/unit/support/assetsRegistryStub.js",
        "^@/assets/(.*)$": "<rootDir>/assets/$1",
        "^@/test/(.*)$": "<rootDir>/tests/unit/support/$1",
        "^@/(.*)$": "<rootDir>/src/$1",
    },
};

/**
 * Two Jest projects so platform-specific modules (`.web.ts(x)` / `.native.ts(x)`) are both exercised:
 * unit tests (`*.test.ts`) run once on the native project; component tests (`*.test.tsx`) run on both.
 * @type {import('jest').Config}
 */
module.exports = {
    // `npm run test:coverage`: the logic folders (gate 80%); views are covered by component and Maestro tests instead.
    // Root-level: in `projects` mode Jest reads coverage settings from the root config, not each project.
    collectCoverageFrom: [
        "src/{stores,services,navigation,utils,hooks}/**/*.{ts,tsx}",
        "!src/**/*.d.ts",
        "!src/**/index.ts",
        "!src/services/**/*.impl.*",
    ],
    // Watchman's file list can lag right after files change (it reports repeated recrawls here), and a full crawl costs little
    watchman: false,
    projects: [
        {
            ...shared,
            displayName: "native",
            preset: "jest-expo/ios",
            testMatch: ["<rootDir>/tests/unit/**/*.test.{ts,tsx,js}"],
        },
        {
            ...shared,
            displayName: "web",
            preset: "jest-expo/web",
            testMatch: ["<rootDir>/tests/unit/**/*.test.tsx"],
        },
    ],
};
