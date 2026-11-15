import {
    Box,
    DropdownMenu,
    Host,
    Icon,
    IconButton,
    Row,
    Text,
} from "@expo/ui/jetpack-compose";
import {
    background,
    clickable,
    clip,
    semantics,
    Shapes,
    size,
    testID,
} from "@expo/ui/jetpack-compose/modifiers";
import type { NativeStackNavigationOptions } from "expo-router/native-stack";
import { useState } from "react";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import { DEFAULT_BACK_TITLE, type OuterBack } from "@/navigation/stackBack";
import { useAppLockStore } from "@/stores/appLock.store";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";

import { HeaderLogo } from "./HeaderLogo";
import { icons, NativeMenuItems } from "./NativeMenuItems.android";
import { useHeaderMenu } from "./useHeaderMenu";

const arrowBack = require("@/assets/icons/android/arrow_back.xml") as number;

/**
 * The back arrow the bar draws for `outerBack` (the platform draws none: the page is first in its own stack), in the
 * navigation slot, as the top app bar shows its own: an arrow, no label (the page's name is its description).
 */
function OuterBackButton({ label, goBack }: OuterBack) {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    return (
        <Host matchContents colorScheme={scheme}>
            <IconButton onClick={goBack} modifiers={[testID("headerBackBtn")]}>
                <Icon
                    source={arrowBack}
                    tint={colors.primary}
                    size={24}
                    contentDescription={
                        label === DEFAULT_BACK_TITLE
                            ? label
                            : `Back to ${label}`
                    }
                />
            </IconButton>
        </Host>
    );
}

/**
 * Android: the top app bar's actions, in Jetpack Compose: the bell, the page's Refresh (when it has pull to refresh), and
 * the account button (the initials in a filled circle) or the settings gear, opening a Material dropdown
 * (`NativeMenuItems`).
 */
function NativeHeaderMenu() {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    const { menu, bell, refresh } = useHeaderMenu();
    const [expanded, setExpanded] = useState(false);
    const close = () => setExpanded(false);
    const open = () => setExpanded(true);
    // Compose menus are popup windows above the whole app, the lock screen and the update dialog included (both React
    // Native views): while either covers the app, the actions go, open menus and submenus with them, as `AppModal`
    // hides its dialogs.
    const locked = useAppLockStore((s) => s.locked);
    const updateRequired = useConfigStore(selectUpdateRequired);
    if (locked || updateRequired) return null;

    return (
        <Host matchContents colorScheme={scheme}>
            <Row verticalAlignment="center">
                {bell ? (
                    <IconButton
                        onClick={bell.onPress}
                        modifiers={[testID("notification-centre-button")]}
                    >
                        <Icon
                            source={icons.bell}
                            tint={colors["bc-grey"]}
                            size={24}
                            contentDescription={bell.accessibilityLabel}
                        />
                    </IconButton>
                ) : null}
                {refresh ? (
                    <IconButton
                        onClick={refresh.onPress}
                        enabled={!refresh.disabled}
                        modifiers={[testID("headerRefreshBtn")]}
                    >
                        <Icon
                            source={icons.refresh}
                            tint={colors["bc-grey"]}
                            size={24}
                            contentDescription="Refresh"
                        />
                    </IconButton>
                ) : null}
                <DropdownMenu
                    expanded={expanded}
                    onDismissRequest={close}
                    color={colors.surface}
                >
                    <DropdownMenu.Trigger>
                        {menu.kind === "account" ? (
                            <Box
                                contentAlignment="center"
                                modifiers={[
                                    size(48, 48),
                                    clip(Shapes.Circle),
                                    clickable(open),
                                    semantics({
                                        contentDescription: menu.buttonLabel,
                                    }),
                                    testID("headerDropdownBtn"),
                                ]}
                            >
                                <Box
                                    contentAlignment="center"
                                    modifiers={[
                                        size(32, 32),
                                        clip(Shapes.Circle),
                                        background(colors.primary),
                                    ]}
                                >
                                    <Text
                                        color={colors["on-primary"]}
                                        style={{
                                            fontSize: 14,
                                            fontWeight: "500",
                                        }}
                                    >
                                        {menu.initials ?? ""}
                                    </Text>
                                </Box>
                            </Box>
                        ) : (
                            <IconButton
                                onClick={open}
                                modifiers={[testID("settingsMenuBtn")]}
                            >
                                <Icon
                                    source={icons.settings}
                                    tint={colors["bc-grey"]}
                                    size={24}
                                    contentDescription={menu.buttonLabel}
                                />
                            </IconButton>
                        )}
                    </DropdownMenu.Trigger>
                    <DropdownMenu.Items>
                        <NativeMenuItems menu={menu} close={close} />
                    </DropdownMenu.Items>
                </DropdownMenu>
            </Row>
        </Host>
    );
}

/**
 * Android: the logo in the top app bar's navigation slot, the drawn back arrow, and the actions above (Compose, from
 * `@expo/ui`). iOS: `nativeHeaderItems.ios.tsx`.
 */
export function useNativeHeaderItems(): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
    back: (target: OuterBack) => NativeStackNavigationOptions;
} {
    return {
        back: (target) => ({
            headerLeft: () => <OuterBackButton {...target} />,
        }),
        logo: { headerLeft: () => <HeaderLogo /> },
        menu: { headerRight: () => <NativeHeaderMenu /> },
    };
}
