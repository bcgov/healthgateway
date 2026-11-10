import { useThemeColors } from "@/hooks/useTheme";

import type { HgSwitchProps } from "./HgSwitch";

// v-switch inset, density compact, color primary (UserProfileNotificationsComponent.vue / Vuetify 4):
// track 52×32 fully rounded, thumb 24 px (scaled to 2/3 when off), thumb centre ±10 px from the track centre.
const TRACK_W = 52;
const TRACK_H = 32;
const THUMB = 24;

/**
 * Web: draws Vuetify's inset switch around a real `<input type="checkbox" role="switch">`, as Vuetify does,
 * so keyboard, screen readers and tests (`input[type=checkbox]`, checked / disabled) behave like the web client.
 */
export function HgSwitch({
    value,
    onValueChange,
    disabled,
    accessibilityLabel,
    testID,
}: HgSwitchProps) {
    const colors = useThemeColors();
    const thumbCentre = TRACK_W / 2 + (value ? 10 : -10);
    return (
        <label
            data-testid={testID}
            style={{
                position: "relative",
                display: "inline-flex",
                alignItems: "center",
                height: 40,
                width: TRACK_W,
                cursor: disabled ? "default" : "pointer",
                // .v-selection-control--disabled
                opacity: disabled ? 0.38 : 1,
                pointerEvents: disabled ? "none" : undefined,
            }}
        >
            <input
                type="checkbox"
                role="switch"
                aria-label={accessibilityLabel}
                checked={value}
                disabled={disabled}
                onChange={(e) => onValueChange(e.target.checked)}
                style={{
                    position: "absolute",
                    inset: 0,
                    width: "100%",
                    height: "100%",
                    margin: 0,
                    opacity: 0,
                    cursor: "inherit",
                    zIndex: 1,
                }}
            />
            <span
                aria-hidden
                style={{
                    width: TRACK_W,
                    height: TRACK_H,
                    borderRadius: 9999,
                    backgroundColor: value
                        ? colors.primary
                        : colors["switch-track"],
                    // .v-switch__track opacity .6; the web client forces the primary (on) track to 1
                    opacity: value ? 1 : 0.6,
                    transition:
                        "background-color 0.2s cubic-bezier(0.4, 0, 0.2, 1)",
                }}
            />
            <span
                aria-hidden
                style={{
                    position: "absolute",
                    top: (40 - THUMB) / 2,
                    left: thumbCentre - THUMB / 2,
                    width: THUMB,
                    height: THUMB,
                    borderRadius: "50%",
                    backgroundColor: colors["switch-thumb"],
                    transform: value ? "none" : "scale(0.6666666667)",
                    transition:
                        "left 0.15s 0.05s cubic-bezier(0, 0, 0.2, 1), transform 0.15s 0.05s cubic-bezier(0, 0, 0.2, 1)",
                    pointerEvents: "none",
                }}
            />
        </label>
    );
}
