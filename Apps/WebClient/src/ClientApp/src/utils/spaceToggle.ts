import { Platform } from "react-native";

interface KeyEvent {
    key: string;
    repeat?: boolean;
    preventDefault: () => void;
}

/**
 * Web only: react-native-web treats Space as activation for roles `button` and `menuitem` only, so a checkbox,
 * radio or chip drawn with a `Pressable` toggles on Enter but not on Space, which keyboard users expect. Spread
 * these onto such a `Pressable` (never onto a `button`, which would fire twice). The key is handled on keydown so
 * the page does not scroll. Native: nothing.
 */
export function spaceToggleProps(
    onToggle: (() => void) | undefined,
    disabled = false
): object {
    if (Platform.OS !== "web" || !onToggle || disabled) return {};
    return {
        onKeyDown: (event: KeyEvent) => {
            if (event.key !== " " && event.key !== "Spacebar") return;
            event.preventDefault();
            if (!event.repeat) onToggle();
        },
    };
}
