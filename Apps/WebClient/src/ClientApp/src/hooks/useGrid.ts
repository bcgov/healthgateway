import { Platform, useWindowDimensions } from "react-native";

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

/** Above this text scale two compact tiles to a row no longer fit their titles, so the grid falls back to one column. */
const MaxCompactFontScale = 1.3;

/**
 * Home's quick-access grid. Below Vuetify's "sm" (600pt), on web and native, it is two columns of compact tiles (icon,
 * title and arrow, as the earlier iOS app had) rather than one column of full cards: a stated deviation from the web
 * client. Elsewhere, and with large text, it is useColumns() with full cards.
 */
export function useCardGrid() {
    const { xs } = useBreakpoint();
    const { fontScale } = useWindowDimensions();
    const columns = useColumns();
    const compact = xs && fontScale <= MaxCompactFontScale;
    return { columns: compact ? 2 : columns, compact };
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
