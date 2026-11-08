import { Pressable, View } from "react-native";

import { AppText, AppVersion, Icon } from "@/components/ui";
import { useThemeColors } from "@/hooks/useTheme";
import { selectUserName, useUserStore } from "@/stores/user.store";

import { DarkModeMenuItem } from "./DarkModeMenuItem";
import { HeaderMenu } from "./HeaderMenu";
import { SecurityMenuItem } from "./SecurityMenuItem";
import { SupportMenuItem } from "./SupportMenuItem";

interface Props {
    visible: boolean;
    showProfile: boolean;
    onClose: () => void;
    onProfile: () => void;
    onLogout: () => void;
    /** Native only: the drawer's Feedback entry (the web client keeps it in the sidebar). */
    onFeedback?: () => void;
    /** Native only: the footer's links (the web client keeps the footer). */
    showSupport?: boolean;
}

/** The v-menu under the avatar button: user name, Profile, Log Out. */
export function UserMenu({
    visible,
    showProfile,
    onClose,
    onProfile,
    onLogout,
    onFeedback,
    showSupport = false,
}: Props) {
    const colors = useThemeColors();
    const userName = useUserStore(selectUserName);
    return (
        <HeaderMenu visible={visible} onClose={onClose}>
            <View className="px-4 py-2">
                <AppText testID="profileUserName" className="font-bold">
                    {userName}
                </AppText>
            </View>
            <View className="h-px bg-border-divider my-1" />
            {showProfile ? (
                <Pressable
                    testID="profileBtn"
                    accessibilityRole="menuitem"
                    onPress={onProfile}
                    className="flex-row items-center px-4 py-3 active:bg-surface-hover"
                >
                    <Icon
                        name="user"
                        size={16}
                        color={colors["bc-grey"]}
                        style={{ width: 24 }}
                    />
                    <AppText>Profile</AppText>
                </Pressable>
            ) : null}
            {/* Health Gateway UI only: the web client has no theme switch. */}
            <DarkModeMenuItem />
            <SecurityMenuItem onClose={onClose} />
            {showSupport ? <SupportMenuItem onClose={onClose} /> : null}
            {onFeedback ? (
                <Pressable
                    testID="menu-btn-feedback-link"
                    accessibilityRole="menuitem"
                    onPress={onFeedback}
                    className="flex-row items-center px-4 py-3 active:bg-surface-hover"
                >
                    <Icon
                        name="comments"
                        size={16}
                        color={colors["bc-grey"]}
                        style={{ width: 24 }}
                    />
                    <AppText>Feedback</AppText>
                </Pressable>
            ) : null}
            <Pressable
                testID="logoutBtn"
                accessibilityRole="menuitem"
                onPress={onLogout}
                className="flex-row items-center px-4 py-3 active:bg-surface-hover"
            >
                <Icon
                    name="right-from-bracket"
                    size={16}
                    color={colors["bc-grey"]}
                    style={{ width: 24 }}
                />
                <AppText>Log Out</AppText>
            </Pressable>
            <View className="h-px bg-border-divider my-1" />
            <AppVersion className="px-4 py-2" />
        </HeaderMenu>
    );
}
