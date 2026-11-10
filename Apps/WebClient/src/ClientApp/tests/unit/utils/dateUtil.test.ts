import { describe, expect, it } from "@jest/globals";

import {
    ascending,
    descending,
    formatDate,
    fromIsoDate,
} from "@/utils/dateUtil";

describe("dateUtil (DateWrapper / DateSortUtility port)", () => {
    it("parses the date part in Pacific time and formats yyyy-MMM-dd in en-US", () => {
        const d = fromIsoDate("2025-07-04T23:30:00Z");
        expect(d.zoneName).toBe("America/Vancouver");
        expect(formatDate(d)).toBe("2025-Jul-04");
        expect(formatDate(fromIsoDate(undefined))).toBe("");
    });

    it("sorts descending with missing dates last, and ascending as its inverse", () => {
        const a = fromIsoDate("2024-01-01");
        const b = fromIsoDate("2025-01-01");
        expect([a, undefined, b].sort(descending)).toEqual([b, a, undefined]);
        expect(descending(undefined, undefined)).toBe(0);
        expect(descending(a, fromIsoDate("2024-01-01"))).toBe(0);
        expect(ascending(a, b)).toBe(-1);
    });
});
