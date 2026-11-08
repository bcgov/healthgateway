import { type ReactNode, useState } from "react";
import { Platform, Pressable, View, type ViewStyle } from "react-native";

import { AppText, Icon } from "@/components/ui";
import { useThemeColors } from "@/hooks/useTheme";

/** Web: `visibility` also takes the hidden options out of the tab order and the accessibility tree. */
const collapsed = {
    height: 0,
    overflow: "hidden",
    ...(Platform.OS === "web" ? { visibility: "hidden" } : {}),
} as ViewStyle;

interface Props {
    label: string;
    icon: string;
    testID: string;
    children: ReactNode;
}

/**
 * A header menu entry that opens in place to show its options, closed each time the menu is reopened. Closed options
 * stay laid out at zero height, so their width still sizes the menu and opening one never resizes it.
 */
export function ExpandableMenuItem({ label, icon, testID, children }: Props) {
    const colors = useThemeColors();
    const [open, setOpen] = useState(false);
    return (
        <View>
            <Pressable
                testID={testID}
                accessibilityRole="menuitem"
                aria-expanded={open}
                onPress={() => setOpen(!open)}
                className="flex-row items-center px-4 py-3 active:bg-surface-hover"
            >
                <Icon
                    name={icon}
                    size={16}
                    color={colors["bc-grey"]}
                    style={{ width: 24 }}
                />
                <AppText className="flex-1">{label}</AppText>
                <Icon
                    name={open ? "chevron-up" : "chevron-down"}
                    size={12}
                    color={colors["bc-grey"]}
                />
            </Pressable>
            <View
                style={open ? undefined : collapsed}
                pointerEvents={open ? "auto" : "none"}
                accessibilityElementsHidden={!open}
                importantForAccessibility={
                    open ? "auto" : "no-hide-descendants"
                }
            >
                {children}
            </View>
        </View>
    );
}
