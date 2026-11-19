// expo-router's own conversion of a native stack's options into the native header's props, as its stacks use it; not
// part of its public API (Expo upgrades: check it is still there, with the same arguments)
import { useHeaderConfigProps } from "expo-router/build/react-navigation/native-stack/views/useHeaderConfigProps";
import { StyleSheet } from "react-native";
import {
    SafeAreaView,
    useSafeAreaInsets,
} from "react-native-safe-area-context";
import { ScreenStack, ScreenStackItem } from "react-native-screens";

import { useThemeColors } from "@/hooks/useTheme";

import { DevBanner } from "./DevBanner";
import { useNativeStackOptions } from "./useNativeStackOptions";

/** The one route of the stack: no title, so its name shows nowhere. */
const ROUTE = { key: "overApp", name: "overApp" } as const;

/**
 * Native: a screen shown over the app (the intro, the lock screen, Update required), which is outside every stack, as
 * the app's pages are: the environment banner (none on Production), then a native stack of its one page, so the
 * platform draws the same navigation bar the app's pages have (`useNativeStackOptions`), where the platform puts it on
 * each device: the logo, and the signed-out settings menu without the app's pages, which are behind this screen
 * (`overApp`). The bar runs to the screen's sides, as the app's do; the content keeps inside the side insets. Covers the
 * status bar's inset itself. Web: `OverAppScreen.web.tsx`.
 */
export function OverAppScreen({ children }: { children: React.ReactNode }) {
    const colors = useThemeColors();
    const insets = useSafeAreaInsets();
    const options = useNativeStackOptions({ overApp: true })({ route: ROUTE });
    const headerConfig = useHeaderConfigProps({
        ...options,
        route: ROUTE,
        // as the app's stacks have it (`NativeStackView`); Android's bar leaves it to this screen
        // (`disableTopInsetApplication`), as it leaves it to the shell
        headerTopInsetEnabled: insets.top !== 0,
        headerHeight: 0,
        headerBack: undefined,
    });
    return (
        <SafeAreaView
            testID="overAppScreen"
            className="flex-1 bg-background"
            edges={["top"]}
        >
            <SafeAreaView edges={["left", "right"]}>
                <DevBanner />
            </SafeAreaView>
            <ScreenStack
                style={{ flex: 1 }}
                nativeContainerStyle={{ backgroundColor: colors.background }}
            >
                <ScreenStackItem
                    screenId={ROUTE.key}
                    activityState={2}
                    style={StyleSheet.absoluteFill}
                    stackPresentation="push"
                    stackAnimation="none"
                    gestureEnabled={false}
                    headerConfig={headerConfig}
                    contentStyle={{ backgroundColor: colors.background }}
                >
                    <SafeAreaView edges={["left", "right"]} style={{ flex: 1 }}>
                        {children}
                    </SafeAreaView>
                </ScreenStackItem>
            </ScreenStack>
        </SafeAreaView>
    );
}
