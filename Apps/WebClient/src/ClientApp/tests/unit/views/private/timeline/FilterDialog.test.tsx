import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { EntryType } from "@/constants/entryType";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import { useTimelineStore } from "@/stores/timeline.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import {
    changeText,
    inputValue,
    isChecked,
    press,
    render,
    screen,
    textOf,
} from "@/test/ui";
import {
    FilterDialog,
    formatFilterCount,
} from "@/views/private/timeline/FilterDialog";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("FilterDialog (FilterComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("counts read as the web client's: K past 999, blank when unknown", () => {
        expect(formatFilterCount(undefined)).toBe("");
        expect(formatFilterCount(0)).toBe("0");
        expect(formatFilterCount(999)).toBe("999");
        expect(formatFilterCount(1234)).toBe("1.2K");
        expect(formatFilterCount(2000)).toBe("2K");
    });

    it("shows the Filter label on a phone too (stated deviation: the web client is icon-only below sm)", async () => {
        setBreakpoint(375);
        await render(
            <FilterDialog
                entryTypes={[EntryType.Immunization]}
                getCount={() => 1}
            />
        );
        expect(textOf(screen.getByTestId("filterDropdown"))).toBe("Filter");
    });

    it("opens pre-filled from the active filter; Cancel leaves the filter alone", async () => {
        useTimelineStore
            .getState()
            .setFilter(
                TimelineFilterBuilder.create()
                    .withKeyword("flu")
                    .withEntryTypes([EntryType.Immunization])
            );
        await render(
            <FilterDialog
                entryTypes={[EntryType.Immunization, EntryType.Medication]}
                getCount={() => 3}
            />
        );
        await press(screen.getByTestId("filterDropdown"));
        expect(services.tracking.trackEvent).toHaveBeenCalled();
        expect(inputValue(screen.getByTestId("filterTextInput"))).toBe("flu");
        expect(
            isChecked(screen.getByTestId(`${EntryType.Immunization}-filter`))
        ).toBe(true);
        expect(
            isChecked(screen.getByTestId(`${EntryType.Medication}-filter`))
        ).toBe(false);

        await changeText(screen.getByTestId("filterTextInput"), "covid");
        await press(screen.getByTestId("btnFilterCancel"));
        expect(useTimelineStore.getState().filter.keyword).toBe("flu");
        // reopening shows the active filter again, not the discarded edit
        await press(screen.getByTestId("filterDropdown"));
        expect(inputValue(screen.getByTestId("filterTextInput"))).toBe("flu");
    });
});
