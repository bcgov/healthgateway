/** Vuetify's `--v-hover-opacity`: the share of the text colour a hovered button or chip lays over itself. */
export const HoverOpacity = 0.04;

/** Vuetify's `--v-focus-opacity`: the same overlay on a control focused from the keyboard (`:focus-visible`). */
export const FocusOpacity = 0.12;

/** WCAG relative luminance of a hex colour, for Vuetify's overlay multiplier. */
export function luma(hex: string): number {
    const channel = (at: number) => {
        const c = parseInt(hex.slice(at, at + 2), 16) / 255;
        return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
    };
    return 0.2126 * channel(1) + 0.7152 * channel(3) + 0.0722 * channel(5);
}

/**
 * Vuetify's overlay opacity: a state's opacity (hover, focus) times `--v-theme-overlay-multiplier`. A `bg-*` fill
 * doubles it when the fill contrasts with the theme: a dark fill (luminance 0.18 or less) in the light theme, a light
 * one in the dark theme. With no fill (text and outlined variants) the root multiplier, 1, applies.
 */
export function overlayOpacity(
    stateOpacity: number,
    fill: string | undefined,
    dark: boolean
): number {
    if (fill === undefined) return stateOpacity;
    const lightFill = luma(fill) > 0.18;
    return stateOpacity * (lightFill === dark ? 2 : 1);
}
