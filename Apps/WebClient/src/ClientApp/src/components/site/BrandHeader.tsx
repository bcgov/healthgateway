import { useWindowDimensions, View } from "react-native";

import HgLogo from "@/assets/images/gov/hg-logo-rev.svg";
import HgLogoDark from "@/assets/images/gov/hg-logo-rev-dark.svg";
import { useColorScheme } from "@/hooks/useTheme";
import { decorativeProps } from "@/utils/decorative";

import { DevBanner } from "./DevBanner";
import { SettingsMenu } from "./SettingsMenu";

/**
 * Native: the environment banner (none on Production), the app bar's logo and the signed-out gear menu, for the screens
 * shown over the app (the intro and the lock screen). It looks like `Header`; the logo is not a link and the menu leaves
 * out the app's own pages, as those are behind these screens (`SettingsMenu` `overApp`). The caller pads the top inset.
 */
export function BrandHeader() {
    const { width } = useWindowDimensions();
    const Logo = useColorScheme() === "dark" ? HgLogoDark : HgLogo;
    // as in Header: v-app-bar's logo link, at most 224px wide
    const logoWidth = Math.min(224, width - 180);
    const logoHeight = logoWidth / 3.1924;
    return (
        <>
            <DevBanner />
            <View
                testID="brandHeader"
                className="bg-app-bar border-b-2 border-border-light flex-row items-center h-16"
            >
                <View className="px-2 h-[62px] justify-center overflow-hidden">
                    <Logo
                        width={logoWidth}
                        height={logoHeight}
                        preserveAspectRatio="xMinYMid meet"
                        // decorative: each screen's own text names the app
                        {...decorativeProps()}
                        style={{
                            width: logoWidth,
                            height: logoHeight,
                            flexShrink: 0,
                        }}
                    />
                </View>
                <View className="flex-1" />
                <SettingsMenu overApp />
            </View>
        </>
    );
}
