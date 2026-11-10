import { Platform } from "react-native";

/**
 * Props that hide a decorative image from the screen reader, for a component that does not translate React Native's
 * accessibility props itself (react-native-svg on web passes unknown props to the DOM, where React rejects them):
 * `aria-hidden` on web, the iOS and Android props on native. A `View` or `Image` takes either; this is for SVGs.
 */
export function decorativeProps(): object {
    return Platform.OS === "web"
        ? { "aria-hidden": true }
        : {
              accessibilityElementsHidden: true,
              importantForAccessibility: "no-hide-descendants",
          };
}

/** Props that make such a component an image with a name (see `decorativeProps`). */
export function imageProps(label: string): object {
    return Platform.OS === "web"
        ? { role: "img", "aria-label": label }
        : { accessibilityRole: "image", accessibilityLabel: label };
}
