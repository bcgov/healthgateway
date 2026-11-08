/**
 * Native animation has no implementation in Jest, and react-native-reanimated/mock loads the real module, which fails
 * here. Just what the app uses: values apply at once, and a timing calls back after its duration.
 */
/* global jest */
const { ScrollView, View } = require("react-native");

const useSharedValue = (initial) => {
    const shared = {
        value: initial,
        get: () => shared.value,
        set: (next) => {
            shared.value =
                typeof next === "function" ? next(shared.value) : next;
        },
    };
    return shared;
};

module.exports = {
    __esModule: true,
    default: { View, ScrollView },
    useSharedValue,
    useAnimatedStyle: (style) => style(),
    withSpring: (to) => to,
    withTiming: (to, config, done) => {
        if (done) setTimeout(() => done(true), config?.duration ?? 0);
        return to;
    },
    useAnimatedScrollHandler: (handler) => handler,
    // tests flip it with jest.mocked(useReducedMotion).mockReturnValue(true)
    useReducedMotion: jest.fn(() => false),
};
