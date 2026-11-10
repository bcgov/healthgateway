import { Platform, Pressable, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import type { ThemeColors } from "@/theme";
import { cn } from "@/utils/cn";

import type { Politeness } from "./announce";
import { AppText } from "./AppText";
import { Icon } from "./Icon";
import { LiveRegion } from "./LiveRegion";

export type AlertType = "info" | "success" | "warning" | "error";

interface Props {
    type: AlertType;
    title?: string;
    text?: string;
    closable?: boolean;
    onClose?: () => void;
    testID?: string;
    className?: string;
    children?: React.ReactNode;
    /** "outlined" (default, HgAlertComponent) or "text": no border, background or padding (v-alert variant="text") */
    variant?: "outlined" | "text";
    /** HgAlertComponent centerContent: icon and wrapped text vertically centred (used without a title) */
    centerContent?: boolean;
    /**
     * Announce the alert when it appears (WCAG 4.1.3): "polite" for the result of an action, "assertive" for an
     * error or a lost connection. Unset (the default) for a notice that is on the page when it loads: v-alert's
     * `role="alert"` on every box made screen readers read them all on page load.
     */
    live?: Politeness;
}

// HgAlertComponent.vue outlined variant: bg / text / border per type; icon aliases from plugins/vuetify.ts
const styles: Record<
    AlertType,
    {
        icon: string;
        border: string;
        bg: string;
        text: string;
        color: keyof ThemeColors;
    }
> = {
    info: {
        icon: "circle-info",
        border: "border-info-text",
        bg: "bg-info-background",
        text: "text-info-text",
        color: "info-text",
    },
    success: {
        icon: "circle-check",
        border: "border-success",
        bg: "bg-success-background",
        text: "text-success",
        color: "success",
    },
    warning: {
        icon: "triangle-exclamation",
        border: "border-warning-text",
        bg: "bg-warning-background",
        text: "text-warning-text",
        color: "warning-text",
    },
    error: {
        icon: "circle-xmark",
        border: "border-error",
        bg: "bg-error-background",
        text: "text-error",
        color: "error",
    },
};

/**
 * Port of HgAlertComponent.vue (v-alert variant="outlined"): 16px padding, v-alert's 28px prepend icon (1.75rem, in
 * Font Awesome's 1.25em-wide box) 16px before the content, which centres on a single line beside it,
 * v-alert-title 1.5rem / 1.75rem weight 500 (BC Sans → Regular), as measured on dev, body text 16px, close icon at the end.
 */
export function HgAlert({
    type,
    title,
    text,
    closable,
    onClose,
    testID,
    className = "",
    children,
    variant = "outlined",
    centerContent,
    live,
}: Props) {
    const colors = useThemeColors();
    const s = styles[type];
    const boxClassName = cn(
        variant === "text"
            ? "flex-row mb-4"
            : cn("flex-row rounded border p-4 mb-4", s.border, s.bg),
        centerContent ? "items-center" : "",
        className
    );
    const content = (
        <>
            <View className={centerContent ? "mr-4" : "mr-4 self-start"}>
                <Icon
                    name={s.icon}
                    size={28}
                    color={colors[s.color]}
                    fixedWidth
                />
            </View>
            <View className={centerContent ? "flex-1" : "flex-1 self-center"}>
                {title ? (
                    <AppText
                        className={cn(
                            "text-[24px] leading-7 tracking-normal",
                            s.text
                        )}
                    >
                        {title}
                    </AppText>
                ) : null}
                {text ? <AppText className={s.text}>{text}</AppText> : null}
                {children}
            </View>
            {closable ? (
                <Pressable
                    accessibilityRole="button"
                    accessibilityLabel="Close"
                    onPress={onClose}
                    // v-alert__close: an x-small icon button (32 by 32, 15px icon), 16px after the content, pulled 2px
                    // up and down so it centres on the 28px icon row
                    className={cn(
                        "ml-4 w-8 h-8 -my-0.5 items-center justify-center",
                        centerContent ? "" : "self-start"
                    )}
                >
                    <Icon
                        name="xmark"
                        size={15}
                        color={colors[s.color]}
                        fixedWidth
                    />
                </Pressable>
            ) : null}
        </>
    );
    if (live)
        return (
            <LiveRegion
                politeness={live}
                testID={testID}
                className={boxClassName}
                announce={[title, text].filter(Boolean).join(". ") || undefined}
            >
                {content}
            </LiveRegion>
        );
    return (
        <View
            testID={testID}
            className={boxClassName}
            // a note, not an alert: read in place, never announced on its own
            role={Platform.OS === "web" ? "note" : undefined}
        >
            {content}
        </View>
    );
}
