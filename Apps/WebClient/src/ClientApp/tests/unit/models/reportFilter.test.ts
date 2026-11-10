import { describe, expect, it } from "@jest/globals";

import { ReportFilterBuilder } from "@/models/reportFilter";
import { formatDate, fromIsoDate, today } from "@/utils/dateUtil";

describe("ReportFilter (models/reportFilter.ts port)", () => {
    it("an empty filter allows every date and has no text", () => {
        const f = ReportFilterBuilder.buildEmpty();
        expect(f.allowsDate(fromIsoDate("1990-01-01"))).toBe(true);
        expect(f.hasActiveFilter()).toBe(false);
        expect(f.filterText).toBe("");
    });

    it("start and end are inclusive days", () => {
        const f = ReportFilterBuilder.create()
            .withStartDate("2024-01-10")
            .withEndDate("2024-01-20")
            .build();
        expect(f.allowsDate(fromIsoDate("2024-01-09"))).toBe(false);
        expect(f.allowsDate(fromIsoDate("2024-01-10"))).toBe(true);
        expect(f.allowsDate(fromIsoDate("2024-01-20"))).toBe(true);
        expect(f.allowsDate(fromIsoDate("2024-01-21"))).toBe(false);
        expect(f.filterText).toBe(
            "Displaying records from 2024-Jan-10 up to 2024-Jan-20"
        );
    });

    it("without an end date the text runs up to today", () => {
        const f = ReportFilterBuilder.create()
            .withStartDate("2024-01-10")
            .build();
        expect(f.filterText).toBe(
            `Displaying records from 2024-Jan-10 up to ${formatDate(today())}`
        );
    });

    it("medications filter", () => {
        const f = ReportFilterBuilder.create().withMedication("A").build();
        expect(f.hasMedicationsFilter()).toBe(true);
        expect(f.allowsMedication("A")).toBe(false);
        expect(f.allowsMedication("B")).toBe(true);
        const copy = ReportFilterBuilder.createFromFilter(f)
            .withMedications([])
            .build();
        expect(copy.hasActiveFilter()).toBe(false);
    });
});
