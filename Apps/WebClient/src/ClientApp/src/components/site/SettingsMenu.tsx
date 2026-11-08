import { useState } from "react";
import { Pressable, View } from "react-native";

import { AppVersion, Icon } from "@/components/ui";
import { useThemeColors } from "@/hooks/useTheme";

import { DarkModeMenuItem } from "./DarkModeMenuItem";
import { HeaderMenu } from "./HeaderMenu";
import { SecurityMenuItem } from "./SecurityMenuItem";
import { SupportMenuItem } from "./SupportMenuItem";
import { useSupportLinksShown } from "./useSupportLinksShown";

/** Native, signed out (stated deviation): a gear in the header in place of the account menu. */
export function SettingsMenu() {
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
            <HeaderMenu visible={open} onClose={close}>
                <DarkModeMenuItem />
                <SecurityMenuItem onClose={close} />
                {supportShown ? <SupportMenuItem onClose={close} /> : null}
                <View className="h-px bg-border-divider my-1" />
                <AppVersion className="px-4 py-2" />
            </HeaderMenu>
        </>
    );
}
