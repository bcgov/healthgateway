import {
    DropdownMenu,
    Host,
    Icon,
    IconButton,
    Row,
} from "@expo/ui/jetpack-compose";
import { testID } from "@expo/ui/jetpack-compose/modifiers";
import type { NativeStackNavigationOptions } from "expo-router/native-stack";
import { useState } from "react";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import { DEFAULT_BACK_TITLE, type OuterBack } from "@/navigation/stackBack";
import { useAppLockStore } from "@/stores/appLock.store";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";

import { HeaderLogo, StaticHeaderLogo } from "./HeaderLogo";
import { icons, NativeMenuItems } from "./NativeMenuItems.android";
import { useHeaderMenu, useSettingsMenu } from "./useHeaderMenu";

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
 * the account or settings button, a gear either way (owner, 2026-11-16: the initials are web's), opening a Material
 * dropdown (`NativeMenuItems`). `overApp`: see `useNativeHeaderItems`.
 */
function NativeHeaderMenu({ overApp }: { overApp: boolean }) {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    const bar = useHeaderMenu();
    const overAppMenu = useSettingsMenu({ overApp: true });
    const { menu, bell, refresh } = overApp ? { menu: overAppMenu } : bar;
    const [expanded, setExpanded] = useState(false);
    const close = () => setExpanded(false);
    const open = () => setExpanded(true);
    // Compose menus are popup windows above the whole app, the lock screen and the update dialog included (both React
    // Native views): while either covers the app, the actions go, open menus and submenus with them, as `AppModal`
    // hides its dialogs. The bar over the app keeps them: it is on the screen that covers the app.
    const locked = useAppLockStore((s) => s.locked);
    const updateRequired = useConfigStore(selectUpdateRequired);
    if (!overApp && (locked || updateRequired)) return null;

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
                        <IconButton
                            onClick={open}
                            modifiers={[
                                testID(
                                    menu.kind === "account"
                                        ? "headerDropdownBtn"
                                        : "settingsMenuBtn"
                                ),
                            ]}
                        >
                            <Icon
                                source={icons.settings}
                                tint={colors["bc-grey"]}
                                size={24}
                                contentDescription={menu.buttonLabel}
                            />
                        </IconButton>
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
 * The options that put `OuterBackButton` on a page's bar. One function for the app's life: `SettingsPage` sets them from
 * an effect that runs again when it changes, and setting them renders it again.
 */
function outerBackOptions(target: OuterBack): NativeStackNavigationOptions {
    return { headerLeft: () => <OuterBackButton {...target} /> };
}

/**
 * Android: the logo in the top app bar's navigation slot, the drawn back arrow, and the actions above (Compose, from
 * `@expo/ui`). `overApp`: the bar of a screen that covers the app (`OverAppScreen`): the logo is not a link, and the
 * settings menu leaves out the app's own pages, which are behind the screen; no bell or Refresh. iOS:
 * `nativeHeaderItems.ios.tsx`.
 */
export function useNativeHeaderItems({
    overApp = false,
}: { overApp?: boolean } = {}): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
    back: (target: OuterBack) => NativeStackNavigationOptions;
} {
    return {
        back: outerBackOptions,
        logo: {
            headerLeft: () => (overApp ? <StaticHeaderLogo /> : <HeaderLogo />),
        },
        menu: { headerRight: () => <NativeHeaderMenu overApp={overApp} /> },
    };
}
