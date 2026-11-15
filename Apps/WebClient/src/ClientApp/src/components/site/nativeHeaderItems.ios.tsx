import type {
    NativeStackHeaderItem,
    NativeStackHeaderItemMenuAction,
    NativeStackHeaderItemMenuSubmenu,
    NativeStackNavigationOptions,
} from "expo-router/native-stack";

import { useThemeColors } from "@/hooks/useTheme";
import {
    type HeaderMenuAction,
    type HeaderMenuEntry,
    type HeaderMenuGlyph,
    nonEmptySections,
} from "@/navigation/headerMenu";
import { useAppLockStore } from "@/stores/appLock.store";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";

import { HeaderLogo } from "./HeaderLogo";
import { useHeaderMenu } from "./useHeaderMenu";

type SFSymbol = Extract<
    NonNullable<NativeStackHeaderItemMenuAction["icon"]>,
    { type: "sfSymbol" }
>["name"];

const sfSymbols: Record<HeaderMenuGlyph, SFSymbol> = {
    bell: "bell",
    darkMode: "moon",
    document: "doc.text",
    email: "envelope",
    external: "arrow.up.right.square",
    feedback: "bubble.left",
    logout: "rectangle.portrait.and.arrow.right",
    profile: "person",
    refresh: "arrow.clockwise",
    security: "lock.shield",
    settings: "gearshape",
    support: "questionmark.circle",
};

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

/**
 * iOS: the navigation bar's own items. The logo sits on the bar without the shared Liquid Glass background (iOS 26);
 * the bell, the page's Refresh (when it has pull to refresh) and the account (initials) or settings (gear) button share
 * one glass capsule, the account or settings menu
 * being a UIMenu: sections become inline groups, Dark Mode and Support submenus. Android: `nativeHeaderItems.android.tsx`.
 */
export function useNativeHeaderItems(): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
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
