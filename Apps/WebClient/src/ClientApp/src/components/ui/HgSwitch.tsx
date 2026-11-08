import { Switch } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";

export interface HgSwitchProps {
    value: boolean;
    onValueChange: (value: boolean) => void;
    disabled?: boolean;
    accessibilityLabel?: string;
    testID?: string;
}

/**
 * Native: the platform switch, tinted with the palette (stated deviation: the web client's Vuetify inset switch
 * is drawn on web only, `HgSwitch.web.tsx`, where parity is required).
 */
export function HgSwitch({
    value,
    onValueChange,
    disabled,
    accessibilityLabel,
    testID,
}: HgSwitchProps) {
    const colors = useThemeColors();
    return (
        <Switch
            testID={testID}
            accessibilityLabel={accessibilityLabel}
            value={value}
            disabled={disabled}
            onValueChange={onValueChange}
            trackColor={{ true: colors.primary, false: colors["switch-track"] }}
        />
    );
}
