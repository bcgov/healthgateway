import { useState } from "react";
import { Pressable, View } from "react-native";

import { AppVersion, Icon } from "@/components/common";
import { useThemeColors } from "@/hooks/useTheme";

import { DarkModeMenuItem } from "./DarkModeMenuItem";
import { HeaderMenu } from "./HeaderMenu";
import { SecurityMenuItem } from "./SecurityMenuItem";
import { SupportMenuItem } from "./SupportMenuItem";
import { useSupportLinksShown } from "./useSupportLinksShown";

/**
 * Native, signed out (stated deviation): a gear in the header in place of the account menu. `overApp`: on a screen that
 * covers the app (the intro, the lock screen, `BrandHeader`), so it leaves out what opens a page of the app, which would
 * open behind the screen: Security (also not to be reached past the lock) and Terms of Service. It also opens while
 * locked.
 */
export function SettingsMenu({ overApp = false }: { overApp?: boolean }) {
    const colors = useThemeColors();
    const supportShown = useSupportLinksShown();
    const [open, setOpen] = useState(false);
    const close = () => setOpen(false);
    return (
        <>
            <Pressable
                testID="settingsMenuBtn"
                accessibilityRole="button"
                accessibilityLabel="Settings"
                onPress={() => setOpen(true)}
                className="w-11 h-11 rounded-full items-center justify-center active:bg-surface-hover mr-1"
            >
                <Icon name="gear" size={20} color={colors["bc-grey"]} />
            </Pressable>
            <HeaderMenu visible={open} onClose={close} overLock={overApp}>
                <DarkModeMenuItem />
                {!overApp ? <SecurityMenuItem onClose={close} /> : null}
                {supportShown ? (
                    <SupportMenuItem onClose={close} externalOnly={overApp} />
                ) : null}
                <View className="h-px bg-border-divider my-1" />
                <AppVersion className="px-4 py-2" />
            </HeaderMenu>
        </>
    );
}
