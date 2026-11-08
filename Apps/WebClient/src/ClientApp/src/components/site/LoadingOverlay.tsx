import { type ReactNode, useEffect, useRef } from "react";
import { ActivityIndicator, Platform, View } from "react-native";

import { AppText, LiveRegion } from "@/components/ui";
import { useThemeColors } from "@/hooks/useTheme";
import {
    selectIsLoading,
    selectLoadingText,
    useLoadingStore,
} from "@/stores/loading.store";

/**
 * Full-screen scrim with the spinner and optional text, like the web client's
 * `<v-overlay>` + `<v-progress-circular size="64" color="primary">` (scrim: on-surface at 32 %).
 * A status region, so the screen reader hears that loading started; `LoadingScope` keeps the keyboard and the
 * reader off the page under it meanwhile.
 */
export function LoadingOverlay() {
    const colors = useThemeColors();
    const isLoading = useLoadingStore(selectIsLoading);
    const text = useLoadingStore(selectLoadingText);
    if (!isLoading) return null;
    return (
        <View
            testID="loadingOverlay"
            pointerEvents="auto"
            // iOS: VoiceOver stays on the overlay while it shows
            accessibilityViewIsModal
            className="absolute inset-0 z-50 items-center justify-center bg-black/30"
            style={{ elevation: 50 }}
        >
            <LiveRegion
                politeness="polite"
                testID="loadingStatus"
                className="items-center"
                announce={text || "Loading"}
            >
                <ActivityIndicator
                    testID="loadingSpinner"
                    size="large"
                    color={colors.primary}
                    // RN's large indicator is ~36pt; the web client's is 64px
                    style={{ transform: [{ scale: 1.75 }] }}
                />
                {text ? (
                    <AppText className="mt-6">{text}</AppText>
                ) : (
                    <AppText className="sr-only">Loading</AppText>
                )}
            </LiveRegion>
        </View>
    );
}

/**
 * The page under `LoadingOverlay`: marked busy while it loads, and taken out of reach. Web: `inert`, so Tab cannot
 * land on a control under the scrim (the focus goes back where it was once loading ends); Android: hidden from
 * TalkBack. iOS relies on the overlay's `accessibilityViewIsModal`.
 */
export function LoadingScope({ children }: { children: ReactNode }) {
    const isLoading = useLoadingStore(selectIsLoading);
    const ref = useRef<React.ComponentRef<typeof View>>(null);
    const focused = useRef<Element | null>(null);
    useEffect(() => {
        if (Platform.OS !== "web") return;
        const element = ref.current as unknown as HTMLElement | null;
        if (!element) return;
        if (isLoading) {
            focused.current = document.activeElement;
            element.setAttribute("inert", "");
            return;
        }
        element.removeAttribute("inert");
        const previous = focused.current as HTMLElement | null;
        focused.current = null;
        if (
            previous?.isConnected &&
            element.contains(previous) &&
            document.activeElement === document.body
        )
            previous.focus({ preventScroll: true });
    }, [isLoading]);
    return (
        <View
            ref={ref}
            className="flex-1"
            aria-busy={isLoading}
            importantForAccessibility={
                isLoading ? "no-hide-descendants" : "auto"
            }
        >
            {children}
        </View>
    );
}
