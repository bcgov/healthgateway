import { createContext, useContext } from "react";
import {
    ActivityIndicator,
    Pressable,
    type PressableProps,
    View,
} from "react-native";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import {
    selectIsLive,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import type { ThemeColors } from "@/theme";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import { elevation1 } from "./elevation";
import { Icon } from "./Icon";

/**
 * Port of the defaults `v-card-actions` (and a v-card's `#actions` slot) gives the buttons inside it: `slim`, 8px
 * side padding instead of 16px and no negative outer margin on the icons. Dialog actions and card actions provide it.
 */
const SlimButtons = createContext(false);

export function CardActions({ children }: { children: React.ReactNode }) {
    return <SlimButtons.Provider value>{children}</SlimButtons.Provider>;
}

export type HgButtonVariant =
    "primary" | "secondary" | "link" | "error" | "error-secondary";

export interface HgButtonProps extends Omit<PressableProps, "children"> {
    /** Off while Health Gateway can't be reached or the login is on hold: Add, Remove and the downloads. */
    requiresConnection?: boolean;
    text: string;
    variant?: HgButtonVariant;
    icon?: string;
    /** v-btn append-icon (a menu's caret) */
    appendIcon?: string;
    loading?: boolean;
    block?: boolean;
    /**
     * Mirrors HgButtonComponent.vue's `uppercase` prop: `false` is the web client's mixed-case button
     * (letter-spacing 0.1px). The default is its uppercase label with 1.25px tracking; the text itself keeps the
     * source's case, only its display changes (`text-transform`).
     */
    uppercase?: boolean;
    /** v-btn `slim` (8px side padding). Defaults to true inside `CardActions`, as Vuetify's v-card-actions does. */
    slim?: boolean;
    className?: string;
    /** extra classes on the label (VppLoginView's `text-body-large` buttons) */
    labelClassName?: string;
    /** forwarded to the button; web uses it for attributes Cypress reads (`webAttributeRef`) */
    ref?: (node: unknown) => void;
}

// Vuetify's elevated (primary, error) and text (link) variants have no border; only outlined ones do.
const container: Record<HgButtonVariant, string> = {
    primary: "bg-primary",
    secondary: "bg-surface border border-primary",
    link: "bg-transparent",
    error: "bg-error",
    "error-secondary": "bg-surface border border-error",
};

const outlined: Record<HgButtonVariant, boolean> = {
    primary: false,
    secondary: true,
    link: false,
    error: false,
    "error-secondary": true,
};

/** v-btn variant="elevated" casts the level-1 shadow while enabled */
const elevated: Record<HgButtonVariant, boolean> = {
    primary: true,
    secondary: false,
    link: false,
    error: true,
    "error-secondary": false,
};

/**
 * v-btn's prepend / append icon at the default size: an 18px FontAwesome glyph in a 1.25em (22.5px) box, 8px from
 * the label (`--v-btn-height / 4.5`) and pulled 4px into the padding on the outside (`/ -9`), which slim removes.
 */
function ButtonIcon({
    name,
    color,
    side,
    slim,
}: {
    name: string;
    color: string;
    side: "prepend" | "append";
    slim: boolean;
}) {
    const outside = slim ? 0 : -4;
    return (
        <View
            className="items-center justify-center"
            style={
                side === "prepend"
                    ? { width: 22.5, marginLeft: outside, marginRight: 8 }
                    : { width: 22.5, marginLeft: 8, marginRight: outside }
            }
        >
            <Icon name={name} size={18} color={color} />
        </View>
    );
}

const label: Record<HgButtonVariant, string> = {
    primary: "text-on-primary",
    secondary: "text-primary",
    link: "text-link",
    error: "text-on-error",
    "error-secondary": "text-error",
};

const iconColor: Record<HgButtonVariant, keyof ThemeColors> = {
    primary: "on-primary",
    secondary: "primary",
    link: "link",
    error: "on-error",
    "error-secondary": "error",
};

/** Port of HgButtonComponent.vue (Vuetify v-btn variants). */
export function HgButton({
    text,
    variant = "primary",
    icon,
    appendIcon,
    loading,
    block,
    disabled,
    requiresConnection,
    uppercase,
    slim: slimProp,
    className = "",
    labelClassName,
    style,
    ...rest
}: HgButtonProps) {
    const slimDefault = useContext(SlimButtons);
    const slim = slimProp ?? slimDefault;
    const colors = useThemeColors();
    const isLive = useConnectivityStore(selectIsLive);
    const isDisabled = disabled || loading || (requiresConnection && !isLive);
    // Light: Vuetify's faded disabled button. Dark: a muted fill with a readable label, because fading the
    // pale-blue fill and its navy label together leaves too little contrast.
    const dark = useColorScheme() === "dark";
    const mutedDisabled = isDisabled && dark;
    const containerClass = mutedDisabled
        ? variant === "link"
            ? container.link
            : outlined[variant]
              ? "bg-disabled-background border border-disabled-background"
              : "bg-disabled-background"
        : container[variant];
    const labelClass = mutedDisabled ? "text-on-disabled" : label[variant];
    const iconTint = mutedDisabled
        ? colors["on-disabled"]
        : colors[iconColor[variant]];
    const mixedCase = uppercase === false;
    const paddingClass = slim ? "px-2" : "px-4";
    // v-btn--disabled drops the elevated shadow
    const shadow = elevated[variant] && !isDisabled ? elevation1 : undefined;
    const trackingClass = mixedCase ? "tracking-[0.1px]" : "tracking-[1.25px]";
    // A hugging button by default; a caller's own alignment (e.g. self-center in a row) replaces it in cn().
    const alignClass = block ? "self-stretch" : "self-start";
    return (
        <Pressable
            accessibilityRole="button"
            aria-disabled={!!isDisabled}
            disabled={isDisabled}
            {...rest}
            // a plain style, not a function: NativeWind on web drops a Pressable's style function
            style={
                shadow
                    ? typeof style === "function"
                        ? (state) => [shadow, style(state)]
                        : [shadow, style]
                    : style
            }
            className={cn(
                "flex-row items-center justify-center rounded py-1.5 min-h-[36px]",
                paddingClass,
                containerClass,
                alignClass,
                isDisabled
                    ? mutedDisabled
                        ? ""
                        : "opacity-50"
                    : "active:opacity-80",
                className
            )}
        >
            {loading ? (
                <ActivityIndicator color={iconTint} className="mr-2" />
            ) : icon ? (
                <ButtonIcon
                    name={icon}
                    color={iconTint}
                    side="prepend"
                    slim={slim}
                />
            ) : null}
            {/* v-btn label: 0.875rem, weight 500 (BC Sans has no 500 → Regular) */}
            <AppText
                className={cn(
                    "font-sans text-[14px] leading-5",
                    trackingClass,
                    !mixedCase && "uppercase",
                    labelClass,
                    labelClassName
                )}
            >
                {text}
            </AppText>
            {appendIcon ? (
                <ButtonIcon
                    name={appendIcon}
                    color={iconTint}
                    side="append"
                    slim={slim}
                />
            ) : null}
        </Pressable>
    );
}
