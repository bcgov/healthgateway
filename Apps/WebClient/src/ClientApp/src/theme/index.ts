import tokens from "./tokens";

export type ThemeColors = Record<keyof typeof tokens.colors, string>;
export type ColorScheme = "light" | "dark";

/** Light palette (web client parity). Prefer `useThemeColors()` in components so dark mode applies. */
export const colors = tokens.colors;
export const darkColors = tokens.darkColors;
export const palettes: Record<ColorScheme, ThemeColors> = {
    light: colors,
    dark: darkColors,
};
export const toCssVars = tokens.toCssVars as (
    palette: ThemeColors
) => Record<string, string>;

/** Vuetify "text-*" scale: [size, line-height] */
export const fontSize = tokens.fontSize as Record<string, string[]>;
export const breakpoints = tokens.breakpoints as Record<
    keyof typeof tokens.breakpoints,
    number
>;

export const fonts = {
    regular: "BCSans-Regular",
    light: "BCSans-Light",
    bold: "BCSans-Bold",
    italic: "BCSans-Italic",
    boldItalic: "BCSans-BoldItalic",
    lightItalic: "BCSans-LightItalic",
} as const;

export const fontAssets = {
    [fonts.regular]: require("@/assets/fonts/BCSans-Regular.ttf") as number,
    [fonts.light]: require("@/assets/fonts/BCSans-Light.ttf") as number,
    [fonts.bold]: require("@/assets/fonts/BCSans-Bold.ttf") as number,
    [fonts.italic]: require("@/assets/fonts/BCSans-Italic.ttf") as number,
    [fonts.boldItalic]:
        require("@/assets/fonts/BCSans-BoldItalic.ttf") as number,
    [fonts.lightItalic]:
        require("@/assets/fonts/BCSans-LightItalic.ttf") as number,
};
