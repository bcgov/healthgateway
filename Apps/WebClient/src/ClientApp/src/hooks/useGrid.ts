import { Platform } from "react-native";

import { useBreakpoint } from "./useBreakpoint";

/**
 * useGrid.ts: cards per row by breakpoint (xl 4, lg 3, md 2, else 1).
 * Native additionally uses two columns from Vuetify's "sm" (600pt): foldables and tablets report
 * phone-like point widths while being physically wide, where a single full-width column reads wrong.
 */
export function useColumns() {
    const { width } = useBreakpoint();
    if (width >= 1920) return 4;
    if (width >= 1280) return 3;
    if (width >= 960) return 2;
    if (Platform.OS !== "web" && width >= 600) return 2;
    return 1;
}

/** The width of one card of the grid, as a percentage. */
export const columnWidth = (columns: number): `${number}%` =>
    columns === 1
        ? "100%"
        : columns === 2
          ? "50%"
          : columns === 3
            ? "33.3333%"
            : "25%";
