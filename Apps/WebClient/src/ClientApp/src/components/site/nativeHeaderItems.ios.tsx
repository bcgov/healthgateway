import type {
    NativeStackHeaderItem,
    NativeStackHeaderItemMenuAction,
    NativeStackHeaderItemMenuSubmenu,
    NativeStackNavigationOptions,
} from "expo-router/native-stack";
import { SymbolView } from "expo-symbols";
import { Pressable, Text } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import {
    type HeaderMenuAction,
    type HeaderMenuEntry,
    type HeaderMenuGlyph,
    nonEmptySections,
} from "@/navigation/headerMenu";
import { DEFAULT_BACK_TITLE, type OuterBack } from "@/navigation/stackBack";
import { useAppLockStore } from "@/stores/appLock.store";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";
import { fonts } from "@/theme";

import { HeaderLogo } from "./HeaderLogo";
import { sfSymbols } from "./headerMenuSymbols";
import { useHeaderMenu } from "./useHeaderMenu";

const icon = (name?: HeaderMenuGlyph) =>
    name ? { type: "sfSymbol" as const, name: sfSymbols[name] } : undefined;

function toAction(entry: HeaderMenuAction): NativeStackHeaderItemMenuAction {
    return {
        type: "action",
        label: entry.label,
        description: entry.subtitle,
        icon: icon(entry.glyph),
        state: entry.checked ? "on" : "off",
        disabled: entry.disabled,
        destructive: entry.destructive,
        onPress: entry.onPress ?? (() => undefined),
    };
}

function toMenuItem(
    entry: HeaderMenuEntry
): NativeStackHeaderItemMenuAction | NativeStackHeaderItemMenuSubmenu {
    if (entry.type === "action") return toAction(entry);
    // one choice at a time: UIKit shows the chosen one under the submenu's name
    return {
        type: "submenu",
        label: entry.label,
        icon: icon(entry.glyph),
        multiselectable: false,
        items: entry.items.map(toAction),
    };
}

/** The system back button's text size (17pt) and its chevron's. */
const BACK_TEXT_SIZE = 17;
const BACK_CHEVRON_SIZE = 20;

/**
 * The back button the bar draws for `outerBack` (UIKit draws none: the page is first in its own stack): the chevron and
 * the name of the page it returns to, as the system back button shows them, in the bar's shared glass on iOS 26. A bar
 * button item cannot show both an image and a title, so it is a custom item.
 */
function OuterBackButton({ label, goBack }: OuterBack) {
    const colors = useThemeColors();
    return (
        <Pressable
            testID="headerBackBtn"
            accessibilityRole="button"
            accessibilityLabel={
                label === DEFAULT_BACK_TITLE ? label : `Back to ${label}`
            }
            onPress={goBack}
            hitSlop={8}
            style={{
                height: 44,
                flexDirection: "row",
                alignItems: "center",
                gap: 4,
                paddingHorizontal: 4,
            }}
        >
            <SymbolView
                name="chevron.backward"
                size={BACK_CHEVRON_SIZE}
                weight="semibold"
                tintColor={colors.primary}
            />
            <Text
                numberOfLines={1}
                style={{
                    fontFamily: fonts.regular,
                    fontSize: BACK_TEXT_SIZE,
                    color: colors.primary,
                }}
            >
                {label}
            </Text>
        </Pressable>
    );
}

/**
 * iOS: the navigation bar's own items. The logo sits on the bar without the shared Liquid Glass background (iOS 26);
 * the bell, the page's Refresh (when it has pull to refresh) and the account (initials) or settings (gear) button share
 * one glass capsule, the account or settings menu
 * being a UIMenu: sections become inline groups, Dark Mode and Support submenus. Android: `nativeHeaderItems.android.tsx`.
 */
export function useNativeHeaderItems(): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
    back: (target: OuterBack) => NativeStackNavigationOptions;
} {
    const colors = useThemeColors();
    const { menu, bell, refresh } = useHeaderMenu();
    // UIMenu is UIKit's, above every React Native view: the lock screen (it can lock in the foreground, after the idle
    // timeout) and the update dialog would not cover an open menu. While either covers the app, the bar's actions go,
    // and the menu presented from them with them, as `AppModal` hides its dialogs (Android: the same).
    const locked = useAppLockStore((s) => s.locked);
    const updateRequired = useConfigStore(selectUpdateRequired);
    const covered = locked || updateRequired;

    const items: NativeStackHeaderItem[] = [];
    if (bell)
        items.push({
            type: "button",
            label: "Notifications",
            icon: icon("bell"),
            tintColor: colors["bc-grey"],
            accessibilityLabel: bell.accessibilityLabel,
            badge: bell.count > 0 ? { value: bell.count } : undefined,
            onPress: bell.onPress,
        });
    if (refresh)
        items.push({
            type: "button",
            label: "Refresh",
            icon: icon("refresh"),
            tintColor: colors.primary,
            accessibilityLabel: "Refresh",
            disabled: refresh.disabled,
            onPress: refresh.onPress,
        });
    items.push({
        type: "menu",
        // the account button shows the initials; the settings button only its gear
        label: menu.initials ?? menu.buttonLabel,
        labelStyle: { fontWeight: "700", color: colors.primary },
        icon: menu.kind === "settings" ? icon("settings") : undefined,
        // a lone icon is drawn as a circle: wider, the gear sits in a pill like the initials (`NativeSettingsMenu`)
        width: menu.kind === "settings" ? 64 : undefined,
        tintColor: colors.primary,
        accessibilityLabel: menu.buttonLabel,
        menu: {
            title: menu.title,
            items: nonEmptySections(menu).map((section) => ({
                type: "submenu",
                label: "",
                inline: true,
                items: section.map(toMenuItem),
            })),
        },
    });

    return {
        back: (target) => ({
            unstable_headerLeftItems: () => [
                { type: "custom", element: <OuterBackButton {...target} /> },
            ],
        }),
        logo: {
            unstable_headerLeftItems: () => [
                {
                    type: "custom",
                    element: <HeaderLogo />,
                    hidesSharedBackground: true,
                },
            ],
        },
        menu: { unstable_headerRightItems: () => (covered ? [] : items) },
    };
}
