import { Platform } from "react-native";

/**
 * Web only: the props that make a `Pressable` or `Text` with `accessibilityRole="link"` render as a real
 * `<a href>`. react-native-web leaves a role `link` to the browser, so Enter does nothing on one without an
 * `href`; with it the browser fires the click, which still runs `onPress`. Call `preventDefault()` there when
 * the press does the navigating itself. Native: nothing (a `View` has no `href`).
 */
export function hrefProps(
    href: string | undefined,
    { newTab = false }: { newTab?: boolean } = {}
): object {
    if (Platform.OS !== "web" || !href) return {};
    return {
        href,
        hrefAttrs: newTab
            ? { target: "_blank", rel: "noopener noreferrer" }
            : undefined,
    };
}
