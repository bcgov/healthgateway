import { Pressable, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import { cn } from "@/utils/cn";
import { spaceToggleProps } from "@/utils/spaceToggle";

import { AppText } from "./AppText";
import { Icon } from "./Icon";
import { webClassRef } from "./webClass";

interface Props {
    children: React.ReactNode;
    /** v-chip in a chip group: elevated primary when selected, outlined primary otherwise */
    selected?: boolean;
    onPress?: () => void;
    /** closable chip (filter labels): the close icon carries `.v-chip__close` on web */
    onClose?: () => void;
    /** the close button's name (default "Close"): say what it removes, e.g. "Remove Keyword filter abc" */
    closeAccessibilityLabel?: string;
    size?: "small" | "default";
    testID?: string;
    accessibilityLabel?: string;
}

/** Vuetify v-chip as the timeline uses it: selectable filter chips and closable filter labels. */
export function HgChip({
    children,
    selected,
    onPress,
    onClose,
    closeAccessibilityLabel = "Close",
    size = "default",
    testID,
    accessibilityLabel,
}: Props) {
    const colors = useThemeColors();
    const selectable = selected !== undefined;
    const height = size === "small" ? 24 : 32;
    return (
        <Pressable
            ref={webClassRef({
                "v-chip": true,
                "v-chip--selected": selected === true,
            })}
            testID={testID}
            accessibilityRole={selectable ? "checkbox" : undefined}
            aria-checked={selectable ? selected : undefined}
            accessibilityLabel={accessibilityLabel}
            disabled={!onPress}
            onPress={onPress}
            {...spaceToggleProps(selectable ? onPress : undefined)}
            style={{ minHeight: height }}
            className={cn(
                "group flex-row items-center rounded-full",
                size === "small" ? "px-2.5" : "px-3",
                selectable
                    ? selected
                        ? "bg-primary"
                        : "border border-primary"
                    : "bg-border-light"
            )}
        >
            {selectable && onPress ? (
                // v-chip__overlay on hover: 4 % of the text colour, doubled on the selected chip's fill, which
                // contrasts with the theme in both palettes (Vuetify's overlay multiplier)
                <View
                    testID="chip-hover-overlay"
                    pointerEvents="none"
                    className={cn(
                        "absolute inset-0 rounded-full opacity-0",
                        selected
                            ? "bg-on-primary group-hover:opacity-[.08]"
                            : "bg-primary group-hover:opacity-[.04]"
                    )}
                />
            ) : null}
            <AppText
                variant="body-medium"
                className={cn(
                    selectable
                        ? selected
                            ? "text-on-primary"
                            : "text-primary"
                        : ""
                )}
            >
                {children}
            </AppText>
            {onClose ? (
                <Pressable
                    ref={webClassRef({ "v-chip__close": true })}
                    accessibilityRole="button"
                    accessibilityLabel={closeAccessibilityLabel}
                    onPress={onClose}
                    hitSlop={6}
                    // a 24px box around the glyph: the web has no hitSlop (WCAG 2.5.8)
                    className="ml-1.5 -mr-1 min-w-6 min-h-6 items-center justify-center"
                >
                    <View>
                        <Icon
                            name="circle-xmark"
                            size={size === "small" ? 14 : 16}
                            color={colors["bc-grey"]}
                        />
                    </View>
                </Pressable>
            ) : null}
        </Pressable>
    );
}
