// @expo/vector-icons loads its font through the asset registry, which Jest does not have.
// Render the glyph name as text so tests can still find icons by name when they need to; the accessibility props
// come through, so tests can see whether an icon is hidden or named.
const React = require("react");
const { Text } = require("react-native");
function FontAwesome6(props) {
    return React.createElement(
        Text,
        {
            testID: props.testID,
            style: props.style,
            accessibilityRole: props.accessibilityRole,
            accessibilityLabel: props.accessibilityLabel,
            accessible: props.accessible,
            accessibilityElementsHidden: props.accessibilityElementsHidden,
            importantForAccessibility: props.importantForAccessibility,
        },
        `icon:${props.name}`
    );
}
FontAwesome6.getImageSource = () =>
    Promise.resolve({ uri: "icon.png", width: 24, height: 24, scale: 1 });
FontAwesome6.StyledIconSet = () => FontAwesome6;
module.exports = FontAwesome6;
module.exports.default = FontAwesome6;
