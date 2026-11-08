import { View } from "react-native";

import { HgRadio } from "@/components/ui";
import { type ThemePreference, useThemeStore } from "@/stores/theme.store";

import { ExpandableMenuItem } from "./ExpandableMenuItem";

const options: {
    value: ThemePreference;
    label: string;
    description?: string;
}[] = [
    { value: "dark", label: "On" },
    { value: "light", label: "Off" },
    {
        value: "system",
        label: "System",
        description:
            "We'll adjust your appearance based on your device's system settings",
    },
];

/**
 * Header menu entry that opens in place to the appearance choices. Health Gateway UI addition (the web client has no
 * theme switch); the choice applies at once and persists on the device (stores/theme.store.ts).
 */
export function DarkModeMenuItem() {
    const preference = useThemeStore((s) => s.preference);
    const setPreference = useThemeStore((s) => s.setPreference);
    return (
        <ExpandableMenuItem
            label="Dark Mode"
            icon="moon"
            testID="menu-btn-dark-mode-link"
        >
            <View
                testID="dark-mode-options"
                role="radiogroup"
                aria-label="Dark Mode"
                className="pl-10 pr-4"
            >
                {options.map(({ value, label, description }) => (
                    <HgRadio
                        key={value}
                        testID={`dark-mode-${label.toLowerCase()}`}
                        label={label}
                        description={description}
                        selected={preference === value}
                        onSelect={() => void setPreference(value)}
                        // keeps the System line wrapping instead of widening the menu
                        className="max-w-[240px]"
                    />
                ))}
            </View>
        </ExpandableMenuItem>
    );
}
