import { describe, expect, it } from "@jest/globals";

import {
    FocusOpacity,
    HoverOpacity,
    overlayOpacity,
} from "@/components/ui/overlay";
import { palettes } from "@/theme";

describe("overlayOpacity", () => {
    it("keeps the state opacity without a fill (text and outlined variants)", () => {
        expect(overlayOpacity(HoverOpacity, undefined, false)).toBe(0.04);
        expect(overlayOpacity(FocusOpacity, undefined, true)).toBe(0.12);
    });

    it("doubles it on a dark fill in the light theme, and not on a light one", () => {
        expect(
            overlayOpacity(HoverOpacity, palettes.light.primary, false)
        ).toBe(0.08);
        expect(
            overlayOpacity(HoverOpacity, palettes.light.surface, false)
        ).toBe(0.04);
    });

    it("doubles it on a light fill in the dark theme, and not on a dark one", () => {
        expect(overlayOpacity(HoverOpacity, palettes.dark.primary, true)).toBe(
            0.08
        );
        expect(overlayOpacity(HoverOpacity, palettes.dark.surface, true)).toBe(
            0.04
        );
    });
});
