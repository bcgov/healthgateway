import { afterEach, describe, expect, it } from "@jest/globals";

import { getDataSource } from "@/constants/dataSource";
import { EntryType } from "@/constants/entryType";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import {
    selectHasActiveFilter,
    selectSelectedEntryTypes,
    useTimelineStore,
} from "@/stores/timeline.store";
import { fromIsoDate } from "@/utils/dateUtil";

describe("timeline store (stores/timeline.ts port)", () => {
    afterEach(() => useTimelineStore.getState().clearFilter());

    it("sets and clears the filter; exposes the selected entry types", () => {
        useTimelineStore
            .getState()
            .setFilter(
                TimelineFilterBuilder.create().withEntryType(
                    EntryType.Immunization
                )
            );
        expect(selectHasActiveFilter(useTimelineStore.getState())).toBe(true);
        expect([
            ...selectSelectedEntryTypes(useTimelineStore.getState()),
        ]).toEqual([EntryType.Immunization]);
        useTimelineStore.getState().clearFilter();
        expect(selectHasActiveFilter(useTimelineStore.getState())).toBe(false);
    });

    it("keeps the linear date and the full-screen entry", () => {
        const date = fromIsoDate("2024-01-30");
        useTimelineStore.getState().setLinearDate(date);
        expect(useTimelineStore.getState().linearDate).toBe(date);
    });
});

describe("getDataSource (DataSourceUtil port)", () => {
    it("maps every entry type to its data source", () => {
        for (const type of Object.values(EntryType))
            expect(getDataSource(type)).toBe(type);
    });
});
