import { describe, expect, it } from "@jest/globals";

import {
    fromIsoFormat,
    isValidDateText,
    maskDate,
    toIsoFormat,
} from "@/components/ui/hgDatePicker.shared";

describe("date picker helpers (HgDatePickerComponent port)", () => {
    it.each([
        ["2", "2"],
        ["2026", "2026-"],
        ["2026o", "2026-O"],
        ["2026oct3", "2026-OCT-3"],
        ["2026-oct-30", "2026-OCT-30"],
        ["2026-oct-301", "2026-OCT-30"],
        ["20x26", "2026-"],
    ])("mask %s → %s", (raw, masked) => {
        expect(maskDate(raw)).toBe(masked);
    });

    it("does not re-add a dash while deleting", () => {
        expect(maskDate("2026", true)).toBe("2026");
        expect(maskDate("2026-OCT", true)).toBe("2026-OCT");
    });

    it("converts between ISO and the display format, and validates", () => {
        expect(fromIsoFormat("2026-10-30")).toBe("2026-OCT-30");
        expect(toIsoFormat("2026-OCT-30")).toBe("2026-10-30");
        expect(isValidDateText("2026-FEB-30")).toBe(false);
        expect(isValidDateText("2026-OCT")).toBe(false);
        expect(isValidDateText("")).toBe(true);
    });
});
