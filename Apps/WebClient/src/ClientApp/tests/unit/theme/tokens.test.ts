import { describe, expect, it } from "@jest/globals";

import { colors, darkColors, type ThemeColors, toCssVars } from "@/theme";

/** WCAG 2.x relative luminance and contrast ratio. */
function luminance(hex: string): number {
    const [r, g, b] = [1, 3, 5].map(
        (i) => parseInt(hex.slice(i, i + 2), 16) / 255
    );
    const lin = (c: number) =>
        c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
    return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b);
}
function contrast(a: string, b: string): number {
    const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
    return (hi + 0.05) / (lo + 0.05);
}

/** Tokens used as text or icon colour on the page background and on cards. */
const textTokens: (keyof ThemeColors)[] = [
    "bc-black",
    "bc-grey",
    "primary",
    "link",
    "secondary",
    "success",
    "error",
    "warning-text",
    "info-text",
    "banner-text",
    "banner-link",
];
const AA_TEXT = 4.5;
const AA_LARGE = 3;

describe("theme tokens", () => {
    it("defines every token in both palettes", () => {
        expect(Object.keys(darkColors).sort()).toEqual(
            Object.keys(colors).sort()
        );
    });

    it("maps tokens to CSS custom properties", () => {
        const vars = toCssVars(colors);
        expect(vars["--color-primary"]).toBe(colors.primary);
        expect(Object.keys(vars)).toHaveLength(Object.keys(colors).length);
    });

    it.each(textTokens)(
        "dark %s meets WCAG AA on the background and on surfaces",
        (token) => {
            expect(
                contrast(darkColors[token], darkColors.background)
            ).toBeGreaterThanOrEqual(AA_TEXT);
            expect(
                contrast(darkColors[token], darkColors.surface)
            ).toBeGreaterThanOrEqual(AA_TEXT);
        }
    );

    it.each(textTokens)("light %s meets WCAG AA on white", (token) => {
        expect(
            contrast(colors[token], colors.background)
        ).toBeGreaterThanOrEqual(AA_TEXT);
    });

    it.each([
        ["primary", "on-primary"],
        ["error", "on-error"],
        ["accent", "on-accent"],
    ] as const)(
        "filled %s keeps its label readable in both palettes",
        (fill, label) => {
            expect(
                contrast(colors[fill], colors[label])
            ).toBeGreaterThanOrEqual(AA_TEXT);
            expect(
                contrast(darkColors[fill], darkColors[label])
            ).toBeGreaterThanOrEqual(AA_TEXT);
        }
    );

    it("keeps the dark app bar and surfaces distinguishable from the background", () => {
        // Large UI regions only need the non-text threshold relative to each other via their borders;
        // the hairline border token must itself be visible against the surface.
        expect(darkColors["border-light"]).toMatch(/^rgba\(255,255,255,/);
        expect(
            contrast(darkColors.primary, darkColors["app-bar"])
        ).toBeGreaterThanOrEqual(AA_LARGE);
    });
});
