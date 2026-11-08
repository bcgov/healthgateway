import { useRouter } from "expo-router";
import { Platform, Pressable } from "react-native";

import { AppText, Icon } from "@/components/ui";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import { toHref } from "@/navigation/href";

/** Lets the panel finish closing first: its native modal must not be on screen when the page changes. */
const CLOSE_DELAY_MS = 400;

/** Native header menu entry (stated deviation: the web client has none): opens the Security page. */
export function SecurityMenuItem({ onClose }: { onClose: () => void }) {
    const router = useRouter();
    const colors = useThemeColors();
    if (Platform.OS === "web") return null;
    return (
        <Pressable
            testID="menu-btn-security-link"
            accessibilityRole="menuitem"
            onPress={() => {
                onClose();
                setTimeout(
                    () => router.push(toHref(Path.Security)),
                    CLOSE_DELAY_MS
                );
            }}
            className="flex-row items-center px-4 py-3 active:bg-surface-hover"
        >
            <Icon
                name="shield"
                size={16}
                color={colors["bc-grey"]}
                style={{ width: 24 }}
            />
            <AppText>Security</AppText>
        </Pressable>
    );
}
