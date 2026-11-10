import { vars } from "nativewind";
import { useEffect } from "react";
import { Platform, useColorScheme as useSystemColorScheme } from "react-native";

import { useThemeStore } from "@/stores/theme.store";
import {
    type ColorScheme,
    palettes,
    type ThemeColors,
    toCssVars,
} from "@/theme";

const cssVars: Record<ColorScheme, Record<string, string>> = {
    light: toCssVars(palettes.light),
    dark: toCssVars(palettes.dark),
};
// NativeWind inline variables: children resolve `var(--color-*)` (every Tailwind colour token) against these.
const themeVars: Record<ColorScheme, ReturnType<typeof vars>> = {
    light: vars(cssVars.light),
    dark: vars(cssVars.dark),
};

/** The scheme in effect: the stored preference, or the OS appearance when the preference is "system". */
export function useColorScheme(): ColorScheme {
    const preference = useThemeStore((s) => s.preference);
    const system = useSystemColorScheme();
    if (preference === "system") return system === "dark" ? "dark" : "light";
    return preference;
}

/** The active palette for inline colour props (icons, spinners, switches). Classes resolve on their own. */
export function useThemeColors(): ThemeColors {
    return palettes[useColorScheme()];
}

/** Style for the app root: sets the colour variables for the whole tree (native and web). */
export function useThemeVars() {
    return themeVars[useColorScheme()];
}

/**
 * Web: react-native-web portals Modals to document.body, outside the root View, so the variables are also
 * set on <html>; color-scheme makes scrollbars and native form controls follow the palette.
 */
export function useWebThemeRoot(scheme: ColorScheme) {
    useEffect(() => {
        if (Platform.OS !== "web" || typeof document === "undefined") return;
        const root = document.documentElement;
        for (const [name, value] of Object.entries(cssVars[scheme])) {
            root.style.setProperty(name, value);
        }
        root.style.colorScheme = scheme;
        root.classList.toggle("dark", scheme === "dark");
    }, [scheme]);
}
