import { useThemeColors } from "@/hooks/useTheme";

import { AppText } from "./AppText";
import { Icon } from "./Icon";

export interface InfoTooltipProps {
    text?: string;
    children?: React.ReactNode;
    /** icon size in px (v-icon small 1.25em ≈ 20, x-small 1em ≈ 14-16) */
    size?: number;
    maxWidth?: number;
    testID?: string;
    nativeID?: string;
    tooltipTestID?: string;
    /** the activator icon (default: the regular circle-question) */
    icon?: string;
    /** default: primary */
    color?: string;
    /** the trigger's name: say what it explains, e.g. "More information about the test status" */
    accessibilityLabel?: string;
}

export function InfoTooltipIcon({
    icon,
    size = 20,
    color,
}: Pick<InfoTooltipProps, "icon" | "size" | "color">) {
    const colors = useThemeColors();
    return (
        <Icon
            name={icon ?? "circle-question"}
            solid={icon !== undefined}
            size={size}
            color={color ?? colors.primary}
        />
    );
}

export function InfoTooltipContent({
    text,
    children,
}: Pick<InfoTooltipProps, "text" | "children">) {
    return text ? (
        <AppText className="text-tooltip-text text-body-medium leading-[1.6]">
            {text}
        </AppText>
    ) : (
        <>{children}</>
    );
}
