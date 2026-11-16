import {
    Button,
    Host,
    Image,
    Menu,
    Picker,
    Section,
    Text,
    VStack,
} from "@expo/ui/swift-ui";
import {
    accessibilityLabel,
    buttonBorderShape,
    buttonStyle,
    controlSize,
    disabled,
    frame,
    pickerStyle,
    tag,
    tint,
} from "@expo/ui/swift-ui/modifiers";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import {
    type HeaderMenuAction,
    type HeaderMenuEntry,
    type HeaderMenuSubmenu,
    nonEmptySections,
} from "@/navigation/headerMenu";

import { sfSymbols } from "./headerMenuSymbols";
import { useSettingsMenu } from "./useHeaderMenu";

/** The gear's frame inside the glass: wider than tall, so the glass is a pill. */
const PILL_ICON_WIDTH = 40;
/** With the glass's padding, the bar's 49pt glass height. */
const PILL_ICON_HEIGHT = 34;
/** The navigation bar gear's glyph size. */
const GLYPH_SIZE = 22;

function MenuAction({ entry }: { entry: HeaderMenuAction }) {
    return (
        <Button
            testID={entry.key}
            label={entry.label}
            systemImage={entry.glyph ? sfSymbols[entry.glyph] : undefined}
            role={entry.destructive ? "destructive" : undefined}
            onPress={entry.onPress}
            modifiers={entry.disabled ? [disabled()] : undefined}
        />
    );
}

/**
 * A submenu of choices (Dark Mode): a menu-style Picker, a nested menu with a checkmark on the chosen one, which shows
 * under the submenu's name, as the navigation bar's UIMenu does (`multiselectable: false`).
 */
function ChoiceSubmenu({ entry }: { entry: HeaderMenuSubmenu }) {
    const chosen = entry.items.find((item) => item.checked)?.key ?? null;
    return (
        <Picker
            testID={entry.key}
            label={entry.label}
            systemImage={entry.glyph ? sfSymbols[entry.glyph] : undefined}
            selection={chosen}
            onSelectionChange={(key: string | null) =>
                entry.items.find((item) => item.key === key)?.onPress?.()
            }
            modifiers={[pickerStyle("menu")]}
        >
            {entry.items.map((item) =>
                // a menu item's second Text is its subtitle
                item.subtitle ? (
                    <VStack key={item.key} modifiers={[tag(item.key)]}>
                        <Text>{item.label}</Text>
                        <Text>{item.subtitle}</Text>
                    </VStack>
                ) : (
                    <Text key={item.key} modifiers={[tag(item.key)]}>
                        {item.label}
                    </Text>
                )
            )}
        </Picker>
    );
}

function MenuEntry({ entry }: { entry: HeaderMenuEntry }) {
    if (entry.type === "action") return <MenuAction entry={entry} />;
    if (entry.items.every((item) => item.checked !== undefined))
        return <ChoiceSubmenu entry={entry} />;
    return (
        <Menu
            testID={entry.key}
            label={entry.label}
            systemImage={entry.glyph ? sfSymbols[entry.glyph] : undefined}
        >
            {entry.items.map((item) => (
                <MenuAction key={item.key} entry={item} />
            ))}
        </Menu>
    );
}

/**
 * iOS: the signed-out settings menu (gear) outside a navigation bar, for `BrandHeader`'s screens over the app: the same
 * menu as the bar's (`nativeHeaderItems.ios.tsx`), drawn by SwiftUI, a glass circle on iOS 26. Unlike the bar's, it
 * stays while the app is locked or an update is required: it is on the screen that covers the app. Android:
 * `NativeSettingsMenu.android.tsx`.
 */
export function NativeSettingsMenu({ overApp = false }: { overApp?: boolean }) {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    const menu = useSettingsMenu({ overApp });
    return (
        <Host matchContents colorScheme={scheme}>
            <Menu
                testID="settingsMenuBtn"
                // wider than tall: a pill, like the signed-in bar's capsule
                label={
                    <Image
                        systemName={sfSymbols.settings}
                        size={GLYPH_SIZE}
                        color={colors.primary}
                        modifiers={[
                            frame({
                                width: PILL_ICON_WIDTH,
                                height: PILL_ICON_HEIGHT,
                            }),
                        ]}
                    />
                }
                modifiers={[
                    buttonStyle("glass"),
                    buttonBorderShape("capsule"),
                    controlSize("regular"),
                    tint(colors.primary),
                    accessibilityLabel(menu.buttonLabel),
                ]}
            >
                {nonEmptySections(menu).map((section) => (
                    <Section key={section[0].key}>
                        {section.map((entry) => (
                            <MenuEntry key={entry.key} entry={entry} />
                        ))}
                    </Section>
                ))}
            </Menu>
        </Host>
    );
}
