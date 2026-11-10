// SVG files import as React components through react-native-svg-transformer; in tests render a plain View.
// react-native-svg on web passes unknown props straight to the DOM, where React rejects React Native's accessibility
// props: the web project fails such a render here, so the mistake shows in Jest rather than in the browser console.
const React = require("react");
const { Platform, View } = require("react-native");
const NATIVE_ONLY = [
    "accessibilityRole",
    "accessibilityLabel",
    "accessibilityElementsHidden",
    "importantForAccessibility",
    "accessible",
];
module.exports = function SvgMock(props) {
    if (Platform.OS === "web") {
        const leaked = NATIVE_ONLY.filter((name) => name in props);
        if (leaked.length > 0)
            throw new Error(
                `SVG rendered with ${leaked.join(", ")}: react-native-svg passes these to the DOM on web. Use decorativeProps() / imageProps() from @/utils/decorative.`
            );
    }
    return React.createElement(View, {
        testID: props.testID ?? "svg",
        accessibilityLabel: props.accessibilityLabel ?? props["aria-label"],
        "aria-hidden": props["aria-hidden"],
    });
};
module.exports.default = module.exports;
