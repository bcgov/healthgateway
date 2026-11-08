/* global jest */
// In-memory AsyncStorage for unit tests.
jest.mock("@react-native-async-storage/async-storage", () =>
    require("@react-native-async-storage/async-storage/jest/async-storage-mock")
);
// Expo Router without a navigation container: tests drive `router` / `routerState` from tests/unit/support/expoRouterMock.
jest.mock("expo-router", () => require("./tests/unit/support/expoRouterMock"));
// Icon font: see tests/unit/support/fontAwesomeMock.js.
jest.mock("@expo/vector-icons/FontAwesome6", () =>
    require("./tests/unit/support/fontAwesomeMock")
);
// The native gesture module has no implementation in Jest (the header menu's swipe to close).
require("react-native-gesture-handler/jestSetup");
// Native animation has no implementation in Jest either (the full-screen entry's swipe back).
jest.mock("react-native-worklets", () =>
    require("./tests/unit/support/workletsMock")
);
jest.mock("react-native-reanimated", () =>
    require("./tests/unit/support/reanimatedMock")
);
// react-native-gesture-handler uses Reanimated when it finds it; in Jest, have it behave as if it isn't there.
jest.mock(
    "react-native-gesture-handler/lib/module/handlers/gestures/reanimatedWrapper",
    () => ({ Reanimated: undefined })
);
// The app logger is silent in tests: failure-path tests would otherwise print every expected error.
require("./src/services/logger").logger.setLevel("silent");
