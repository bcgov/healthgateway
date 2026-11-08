import FontAwesome6 from "@expo/vector-icons/FontAwesome6";
import { NativeTabs } from "expo-router/native-tabs";
import {
    type ColorValue,
    type ImageSourcePropType,
    Platform,
} from "react-native";

import { Action, Type } from "@/constants/tracking";
import { useThemeColors } from "@/hooks/useTheme";
import {
    menuEvents,
    menuItems,
    menuRouteName,
    visibleMenuItems,
} from "@/navigation/menu";
import { isFullyRegisteredUser } from "@/navigation/userState";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { selectFeatureToggles, useConfigStore } from "@/stores/config.store";
import { useUserStore } from "@/stores/user.store";

/** The FontAwesome 6 set resolves the glyph's own style (these are all solid), as the drawer icons do. */
const fontAwesome = FontAwesome6 as unknown as {
    getImageSource: (
        name: string,
        size: number,
        color: ColorValue
    ) => Promise<ImageSourcePropType | null>;
};

/**
 * Native main navigation: the platform bottom tab bar (UITabBar with Liquid Glass on iOS 26, Material tabs
 * on Android) with the drawer's items. Web: `_layout.web.tsx`, the drawer stays.
 * Tab visibility is fixed before mount: the configuration is loaded during bootstrap.
 */
export default function TabsLayout() {
    const colors = useThemeColors();
    const toggles = useConfigStore(selectFeatureToggles);
    const visible = new Set(visibleMenuItems(toggles).map((m) => m.path));
    // subscribe so the bar follows sign-in / registration state (SidebarComponent.vue's v-if)
    useAuthStore((s) => s.isAuthenticated);
    useUserStore((s) => s.user);
    useUserStore((s) => s.patientRetrievalFailed);
    const showBar = isFullyRegisteredUser();

    return (
        <NativeTabs
            hidden={!showBar}
            minimizeBehavior="onScrollDown"
            tintColor={colors.primary}
            iconColor={colors["bc-grey"]}
            labelStyle={{ color: colors["bc-grey"] }}
            // Android: the Material bar keeps the OS theme's background when the in-app appearance changes, so
            // paint it from the palette like the drawer. iOS keeps its blur / Liquid Glass (the prop would make
            // the iOS 18 bar opaque).
            backgroundColor={
                Platform.OS === "android" ? colors.surface : undefined
            }
            indicatorColor={colors["nav-highlight"]}
            // Android: Material shows only the selected label by default; keep all five like the web menu
            labelVisibilityMode="labeled"
        >
            {menuItems.map((item) => (
                <NativeTabs.Trigger
                    key={item.path}
                    name={menuRouteName(item.path)}
                    hidden={!visible.has(item.path)}
                    testID={item.testID}
                    accessibilityLabel={item.title}
                    listeners={{
                        tabPress: () => {
                            const e = menuEvents[item.path];
                            if (e)
                                services.tracking.trackEvent({
                                    action: Action.InternalLink,
                                    type: Type.Sidebar,
                                    ...e,
                                });
                        },
                    }}
                >
                    <NativeTabs.Trigger.Label>
                        {item.title}
                    </NativeTabs.Trigger.Label>
                    {/* a VectorIcon only renders as the Icon's src; template mode lets the bar tint it */}
                    <NativeTabs.Trigger.Icon
                        renderingMode="template"
                        src={
                            <NativeTabs.Trigger.VectorIcon
                                family={fontAwesome}
                                name={item.icon}
                            />
                        }
                    />
                </NativeTabs.Trigger>
            ))}
        </NativeTabs>
    );
}
