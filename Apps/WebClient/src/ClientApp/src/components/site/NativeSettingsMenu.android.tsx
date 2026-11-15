import { DropdownMenu, Host, Icon, IconButton } from "@expo/ui/jetpack-compose";
import { testID } from "@expo/ui/jetpack-compose/modifiers";
import { useState } from "react";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";

import { icons, NativeMenuItems } from "./NativeMenuItems.android";
import { useSettingsMenu } from "./useHeaderMenu";

/**
 * Android: the signed-out settings menu (gear) outside a top app bar, for `BrandHeader`'s screens over the app: the same
 * Material dropdown as the bar's (`nativeHeaderItems.android.tsx`). Unlike the bar's, it stays while the app is locked
 * or an update is required: it is on the screen that covers the app. iOS: `NativeSettingsMenu.ios.tsx`.
 */
export function NativeSettingsMenu({ overApp = false }: { overApp?: boolean }) {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    const menu = useSettingsMenu({ overApp });
    const [expanded, setExpanded] = useState(false);
    const close = () => setExpanded(false);
    return (
        <Host matchContents colorScheme={scheme}>
            <DropdownMenu
                expanded={expanded}
                onDismissRequest={close}
                color={colors.surface}
            >
                <DropdownMenu.Trigger>
                    <IconButton
                        onClick={() => setExpanded(true)}
                        modifiers={[testID("settingsMenuBtn")]}
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
        </Host>
    );
}
