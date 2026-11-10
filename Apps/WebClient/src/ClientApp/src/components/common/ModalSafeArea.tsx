import type { ReactNode } from "react";
import { Platform, View } from "react-native";
import {
    SafeAreaProvider,
    useSafeAreaInsets,
} from "react-native-safe-area-context";

/** `both` for a full-screen modal; `right` for a panel docked to the right, whose left side is mid-screen. */
type Sides = "both" | "right";

function Padded({ children, sides }: { children: ReactNode; sides: Sides }) {
    const insets = useSafeAreaInsets();
    return (
        <View
            className="flex-1"
            style={{
                paddingTop: insets.top,
                paddingBottom: insets.bottom,
                paddingLeft: sides === "both" ? insets.left : 0,
                paddingRight: insets.right,
            }}
        >
            {children}
        </View>
    );
}

/**
 * Pads full-screen modal content clear of the status bar and home indicator, and in landscape of the camera and the
 * clock at the side (the home indicator stays at the bottom). A Modal is a separate native window, so
 * the screen's insets do not apply inside it (on iOS they read 0 at the top, putting controls under the clock); its
 * own SafeAreaProvider measures the modal window instead, as react-native-safe-area-context documents for modals.
 */
export function ModalSafeArea({
    children,
    sides = "both",
}: {
    children: ReactNode;
    sides?: Sides;
}) {
    // web has no status bar to clear, and inside a transformed parent the provider measures a bottom inset as tall as the page
    if (Platform.OS === "web")
        return <View className="flex-1">{children}</View>;
    return (
        <SafeAreaProvider>
            <Padded sides={sides}>{children}</Padded>
        </SafeAreaProvider>
    );
}
