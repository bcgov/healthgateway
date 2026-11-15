import {
    DropdownMenu,
    DropdownMenuItem,
    HorizontalDivider,
    Icon,
    Text,
} from "@expo/ui/jetpack-compose";
import { testID } from "@expo/ui/jetpack-compose/modifiers";
import { Fragment, useState } from "react";
import type { ImageSourcePropType } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import {
    type HeaderMenu,
    type HeaderMenuAction,
    type HeaderMenuEntry,
    type HeaderMenuGlyph,
    type HeaderMenuSubmenu,
    nonEmptySections,
} from "@/navigation/headerMenu";

/** Compose's `Icon` draws vector drawables only. */
export const icons: Record<HeaderMenuGlyph, ImageSourcePropType> = {
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
 * Android: a header menu's entries, inside a Compose `DropdownMenu`, for the navigation bar's menu
 * (`nativeHeaderItems.android.tsx`) and `NativeSettingsMenu`. Material menus have no title, so the menu's title (the
 * user's name) is the first, inactive line; sections are split by dividers.
 */
export function NativeMenuItems({
    menu,
    close,
}: ItemProps & { menu: HeaderMenu }) {
    const colors = useThemeColors();
    const sections = nonEmptySections(menu);
    return (
        <>
            {menu.title ? (
                <DropdownMenuItem
                    enabled={false}
                    modifiers={[testID("profileUserName")]}
                >
                    <DropdownMenuItem.Text>
                        <Text color={colors["bc-grey"]}>{menu.title}</Text>
                    </DropdownMenuItem.Text>
                </DropdownMenuItem>
            ) : null}
            {sections.map((section, i) => (
                <Fragment key={section[0]?.key ?? i}>
                    {i > 0 || menu.title ? (
                        <HorizontalDivider color={colors["border-divider"]} />
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
        </>
    );
}
