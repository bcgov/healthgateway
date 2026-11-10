import { describe, expect, it } from "@jest/globals";

import { Covid19TestResultTimelineEntry } from "@/models/timeline/covid19TestResultTimelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { covid19OrdersFixture } from "@/test/fixtures";
import { formatDate } from "@/utils/dateUtil";

describe("Covid19TestResultTimelineEntry (covid19TestResultTimelineEntry.ts port)", () => {
    it("summarises the newest test; the entry date is the first listed result's collection", () => {
        const e = new Covid19TestResultTimelineEntry(covid19OrdersFixture()[1]);
        expect(e.tests.map((t) => t.id)).toEqual(["b", "a"]);
        expect(formatDate(e.displayDate)).toBe("2021-Mar-02");
        expect(formatDate(e.date)).toBe("2021-Mar-01");
        expect(e.summaryTitle).toBe("SARS-CoV-2 PCR");
        expect(e.resultReady).toBe(false);
        expect(e.tests[1].outOfRange).toBe("False");
    });

    it("keyword search covers the summary title and test type", () => {
        const e = new Covid19TestResultTimelineEntry(covid19OrdersFixture()[0]);
        expect(
            e.filterApplies(
                TimelineFilterBuilder.create().withKeyword("pcr").build()
            )
        ).toBe(true);
        expect(
            e.filterApplies(
                TimelineFilterBuilder.create().withKeyword("nat").build()
            )
        ).toBe(true);
        expect(
            e.filterApplies(
                TimelineFilterBuilder.create().withKeyword("lifelabs").build()
            )
        ).toBe(false);
    });
});
