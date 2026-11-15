import {
    Box,
    DropdownMenu,
    DropdownMenuItem,
    HorizontalDivider,
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
import { Fragment, useState } from "react";
import type { ImageSourcePropType } from "react-native";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import {
    type HeaderMenuAction,
    type HeaderMenuEntry,
    type HeaderMenuGlyph,
    type HeaderMenuSubmenu,
    nonEmptySections,
} from "@/navigation/headerMenu";
import { useAppLockStore } from "@/stores/appLock.store";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";

import { HeaderLogo } from "./HeaderLogo";
import { useHeaderMenu } from "./useHeaderMenu";

/** Compose's `Icon` draws vector drawables only. */
const icons: Record<HeaderMenuGlyph, ImageSourcePropType> = {
    bell: require("@/assets/icons/android/bell.xml") as number,
    darkMode: require("@/assets/icons/android/dark_mode.xml") as number,
    document: require("@/assets/icons/android/description.xml") as number,
    email: require("@/assets/icons/android/mail.xml") as number,
    external: require("@/assets/icons/android/open_in_new.xml") as number,
    feedback: require("@/assets/icons/android/feedback.xml") as number,
    logout: require("@/assets/icons/android/logout.xml") as number,
    profile: require("@/assets/icons/android/person.xml") as number,
    refresh: require("@/assets/icons/android/refresh.xml") as number,
    security: require("@/assets/icons/android/security.xml") as number,
    settings: require("@/assets/icons/android/settings.xml") as number,
    support: require("@/assets/icons/android/help.xml") as number,
};
const checkIcon = require("@/assets/icons/android/check.xml") as number;
const submenuIcon = require("@/assets/icons/android/arrow_right.xml") as number;

interface ItemProps {
    /** Closes the whole menu, from a submenu too. */
    close: () => void;
}

function MenuAction({ entry, close }: ItemProps & { entry: HeaderMenuAction }) {
    const colors = useThemeColors();
    const textColor = entry.disabled ? colors["bc-grey"] : colors["bc-black"];
    return (
        <DropdownMenuItem
            enabled={!entry.disabled}
            modifiers={[testID(entry.key)]}
            onClick={() => {
                close();
                entry.onPress?.();
            }}
        >
            <DropdownMenuItem.Text>
                <Text color={textColor}>{entry.label}</Text>
            </DropdownMenuItem.Text>
            {entry.glyph ? (
                <DropdownMenuItem.LeadingIcon>
                    <Icon
                        source={icons[entry.glyph]}
                        tint={colors["bc-grey"]}
                        size={24}
                    />
                </DropdownMenuItem.LeadingIcon>
            ) : null}
            {entry.checked ? (
                <DropdownMenuItem.TrailingIcon>
                    <Icon
                        source={checkIcon}
                        tint={colors.primary}
                        size={24}
                        contentDescription="Selected"
                    />
                </DropdownMenuItem.TrailingIcon>
            ) : null}
        </DropdownMenuItem>
    );
}

/** A submenu opens as its own dropdown over the menu, as Material's cascading menus do. */
function MenuSubmenu({
    entry,
    close,
}: ItemProps & { entry: HeaderMenuSubmenu }) {
    const colors = useThemeColors();
    const [expanded, setExpanded] = useState(false);
    const closeAll = () => {
        setExpanded(false);
        close();
    };
    return (
        <DropdownMenu
            expanded={expanded}
            onDismissRequest={() => setExpanded(false)}
            color={colors.surface}
        >
            <DropdownMenu.Trigger>
                <DropdownMenuItem
                    modifiers={[testID(entry.key)]}
                    onClick={() => setExpanded(true)}
                >
                    <DropdownMenuItem.Text>
                        <Text color={colors["bc-black"]}>{entry.label}</Text>
                    </DropdownMenuItem.Text>
                    {entry.glyph ? (
                        <DropdownMenuItem.LeadingIcon>
                            <Icon
                                source={icons[entry.glyph]}
                                tint={colors["bc-grey"]}
                                size={24}
                            />
                        </DropdownMenuItem.LeadingIcon>
                    ) : null}
                    <DropdownMenuItem.TrailingIcon>
                        <Icon
                            source={submenuIcon}
                            tint={colors["bc-grey"]}
                            size={24}
                        />
                    </DropdownMenuItem.TrailingIcon>
                </DropdownMenuItem>
            </DropdownMenu.Trigger>
            <DropdownMenu.Items>
                {entry.items.map((item) => (
                    <MenuAction key={item.key} entry={item} close={closeAll} />
                ))}
            </DropdownMenu.Items>
        </DropdownMenu>
    );
}

function MenuEntry({ entry, close }: ItemProps & { entry: HeaderMenuEntry }) {
    return entry.type === "submenu" ? (
        <MenuSubmenu entry={entry} close={close} />
    ) : (
        <MenuAction entry={entry} close={close} />
    );
}

/**
 * Android: the top app bar's actions, in Jetpack Compose: the bell, the page's Refresh (when it has pull to refresh), and
 * the account button (the initials in a filled
 * circle) or the settings gear, opening a Material dropdown. Material menus have no title, so the user's name is the
 * first, inactive line; sections are split by dividers.
 */
function NativeHeaderMenu() {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    const { menu, bell, refresh } = useHeaderMenu();
    const [expanded, setExpanded] = useState(false);
    const close = () => setExpanded(false);
    const open = () => setExpanded(true);
    const sections = nonEmptySections(menu);
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
                        {menu.title ? (
                            <DropdownMenuItem
                                enabled={false}
                                modifiers={[testID("profileUserName")]}
                            >
                                <DropdownMenuItem.Text>
                                    <Text color={colors["bc-grey"]}>
                                        {menu.title}
                                    </Text>
                                </DropdownMenuItem.Text>
                            </DropdownMenuItem>
                        ) : null}
                        {sections.map((section, i) => (
                            <Fragment key={section[0]?.key ?? i}>
                                {i > 0 || menu.title ? (
                                    <HorizontalDivider
                                        color={colors["border-divider"]}
                                    />
                                ) : null}
                                {section.map((entry) => (
                                    <MenuEntry
                                        key={entry.key}
                                        entry={entry}
                                        close={close}
                                    />
                                ))}
                            </Fragment>
                        ))}
                    </DropdownMenu.Items>
                </DropdownMenu>
            </Row>
        </Host>
    );
}

/**
 * Android: the logo in the top app bar's navigation slot, and the actions above (Compose, from `@expo/ui`). iOS:
 * `nativeHeaderItems.ios.tsx`.
 */
export function useNativeHeaderItems(): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
} {
    return {
        logo: { headerLeft: () => <HeaderLogo /> },
        menu: { headerRight: () => <NativeHeaderMenu /> },
    };
}
