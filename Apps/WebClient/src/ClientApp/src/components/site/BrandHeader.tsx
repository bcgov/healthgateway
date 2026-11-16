import { getDefaultHeaderHeight } from "expo-router/react-navigation";
import { useRef, useState } from "react";
import { Platform, StyleSheet, useWindowDimensions, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import { useNavBarStore } from "@/stores/navBar.store";

import { DevBanner } from "./DevBanner";
import { HeaderLogoImage } from "./HeaderLogo";
import { NativeSettingsMenu } from "./NativeSettingsMenu";

/** The bar's leading and trailing margin before a real bar has been measured: UIKit's and Material's on phones. */
const DEFAULT_MARGIN = 16;
/** UIKit's default navigation bar shadow: a hairline of black at 30%. */
const IOS_BAR_SHADOW = "rgba(0, 0, 0, 0.3)";

/**
 * Native: the environment banner (none on Production) and a bar drawn like the navigation bar (`useNativeStackOptions`),
 * for the screens shown over the app (the intro, the lock screen, Update required), which are outside every stack: the
 * bar's logo and its signed-out settings menu, the same native menu. Its height and margins are the real bar's, measured
 * under these screens (`useNavBarStore`). The logo is not a link and the menu leaves out the app's own pages, as those
 * are behind these screens (`overApp`). The caller pads the top inset.
 */
export function BrandHeader() {
    const colors = useThemeColors();
    const window = useWindowDimensions();
    const measured = useNavBarStore();
    // the platform bar's height, without the status bar (the caller's inset), until the real one is known
    const height = measured.height ?? getDefaultHeaderHeight(window, false, 0);
    // the bar's margins are from the window's edges; this bar may start inside them (the caller's side insets)
    const ref = useRef<React.ComponentRef<typeof View>>(null);
    const [offset, setOffset] = useState({ left: 0, right: 0 });
    const margin = measured.leading ?? DEFAULT_MARGIN;
    return (
        <>
            <DevBanner />
            <View
                ref={ref}
                testID="brandHeader"
                onLayout={() =>
                    ref.current?.measureInWindow((x, _y, width) =>
                        setOffset({
                            left: x,
                            right: Math.max(0, window.width - x - width),
                        })
                    )
                }
                style={{
                    height,
                    paddingLeft: Math.max(0, margin - offset.left),
                    // the same margin on the right: UIKit's bar is symmetric, and the glass gear's edge then sits where the
                    // bar's does
                    paddingRight: Math.max(0, margin - offset.right),
                    backgroundColor: colors["app-bar"],
                    // the bar's shadow (`headerShadowVisible`): a hairline on iOS, an elevation on Android
                    ...Platform.select({
                        ios: {
                            borderBottomWidth: StyleSheet.hairlineWidth,
                            borderBottomColor: IOS_BAR_SHADOW,
                        },
                        android: { elevation: 4 },
                    }),
                }}
                className="flex-row items-center"
            >
                <HeaderLogoImage />
                <View className="flex-1" />
                <NativeSettingsMenu overApp />
            </View>
        </>
    );
}
