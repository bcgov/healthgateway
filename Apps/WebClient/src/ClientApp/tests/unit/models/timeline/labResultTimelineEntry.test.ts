import { describe, expect, it } from "@jest/globals";

import { LabResultTimelineEntry } from "@/models/timeline/labResultTimelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { laboratoryOrdersFixture } from "@/test/fixtures";
import { formatDate } from "@/utils/dateUtil";

describe("LabResultTimelineEntry (labResultTimelineEntry.ts port)", () => {
    const entry = new LabResultTimelineEntry(laboratoryOrdersFixture()[0]);

    it("maps the order and sorts tests: out of range, in range, then the rest", () => {
        expect(entry.id).toBe("lab1");
        expect(formatDate(entry.date, "yyyy-MMM-dd HH:mm")).toBe(
            "2024-May-01 09:30"
        );
        expect(entry.reportingLab).toBe("LifeLabs");
        expect(entry.tests.map((t) => t.testName)).toEqual([
            "LDL",
            "HDL",
            "TG",
        ]);
    });

    it("status info text per status", () => {
        expect(entry.tests[0].statusInfo).toEqual([
            "Download the PDF to see your detailed results.",
        ]);
        expect(entry.tests[2].statusInfo[0]).toBe(
            "Most results are ready about 3 days after your test."
        );
    });

    it("keyword search covers name, lab and test names", () => {
        for (const k of ["lipid", "lifelabs", "ldl"])
            expect(
                entry.filterApplies(
                    TimelineFilterBuilder.create().withKeyword(k).build()
                )
            ).toBe(true);
    });
});
