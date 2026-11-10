import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { PageScroll } from "@/components/site/PageScroll";
import { EntryType } from "@/constants/entryType";
import type {
    ImmunizationEvent,
    ImmunizationResult,
} from "@/models/immunization";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { useTimelineStore } from "@/stores/timeline.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    encountersFixture,
    fakeAuth,
    immunizationResultFixture,
    laboratoryOrdersFixture,
    medicationStatementsFixture,
    requestResult,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    changeText,
    headingLevels,
    press,
    render,
    screen,
    textOf,
    unmount,
    waitFor,
} from "@/test/ui";
import { Timeline } from "@/views/private/timeline/Timeline";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

/** n immunizations on consecutive days of January 2024, the newest on the 30th */
const many = (n: number): ImmunizationResult => ({
    recommendations: [],
    immunizations: Array.from({ length: n }, (_, i): ImmunizationEvent => ({
        id: `i${i}`,
        status: "Completed",
        dateOfImmunization: `2024-01-${String(30 - i).padStart(2, "0")}T00:00:00`,
        providerOrClinic: "Clinic",
        immunization: { name: `Dose ${i}`, immunizationAgents: [] },
    })),
});

describe("Timeline (TimelineComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
    });
    afterEach(async () => {
        await unmount();
        resetStores();
        useTimelineStore.setState({ fullscreenEntry: undefined });
        jest.restoreAllMocks();
    });

    it("lists the immunizations newest first with the record count", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 1 to 2 out of 2 records"
        );
        expect(screen.getAllByTestId("entryCardDate").map(textOf)).toEqual([
            "2023-Oct-15",
            "2021-Mar-01",
        ]);
        expect(screen.getAllByTestId("timelineCard")).toHaveLength(2);
        expect(
            screen.getAllByTestId("entry-card-type-title").map(textOf)
        ).toEqual(["Immunizations", "Immunizations"]);
    });

    it("pages 25 at a time", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(many(30));
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 1 to 25 out of 30 records"
        );
        expect(textOf(screen.getAllByTestId("entryCardDate")[0])).toBe(
            "2024-Jan-30"
        );
        await press(screen.getByLabelText("Next page"));
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 26 to 30 out of 30 records"
        );
        expect(textOf(screen.getAllByTestId("entryCardDate")[0])).toBe(
            "2024-Jan-05"
        );
        await press(screen.getByLabelText("Previous page"));
        expect(textOf(screen.getAllByTestId("entryCardDate")[0])).toBe(
            "2024-Jan-30"
        );
    });

    it("no records: the empty state", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue({
            immunizations: [],
            recommendations: [],
        });
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        expect(textOf(screen.getByTestId("noTimelineEntriesText"))).toBe(
            "No records found"
        );
        expect(screen.queryByTestId("pagination")).toBeNull();
    });

    it("shows the skeleton and the loading toast while loading", async () => {
        let resolve: (r: ImmunizationResult) => void = () => undefined;
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockReturnValue(new Promise((r) => (resolve = r)));
        await render(
            <PageScroll>
                <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
            </PageScroll>
        );
        expect(screen.getByTestId("content-placeholders")).toBeTruthy();
        expect(screen.getByTestId("loading-toast")).toBeTruthy();
        // native: the short pill; web: the web client's toast
        expect(
            screen.getByText(
                Platform.OS === "web"
                    ? "Retrieving your health records"
                    : "Retrieving…"
            )
        ).toBeTruthy();
        resolve(immunizationResultFixture);
        await waitFor(() =>
            expect(screen.queryByTestId("loading-toast")).toBeNull()
        );
        expect(screen.queryByTestId("content-placeholders")).toBeNull();
    });

    it("a blocked data source is not requested and adds the unavailable warning", async () => {
        const get = jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        );
        signInRegistered({ blockedDataSources: ["Immunization"] });
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        expect(get).not.toHaveBeenCalled();
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Immunizations are unavailable at this time. Please try again later."
        );
    });

    it("desktop: pressing a card shows its details; mobile: it opens the full-screen details", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        expect(screen.queryByText("Comirnaty")).toBeNull();
        await press(screen.getByLabelText(/^2021-Mar-01/));
        expect(screen.getByTestId("immunizationProductTitle")).toBeTruthy();
        expect(screen.getByText("Comirnaty")).toBeTruthy();

        // collapsed first: on native a card left open would carry on full screen (tests/unit/views/private/timeline/entryAcrossFold)
        await press(screen.getByLabelText(/^2021-Mar-01/));
        updateStores(() => setBreakpoint(390));
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        await press(screen.getByLabelText(/^2021-Mar-01/));
        await waitFor(() =>
            expect(screen.getByTestId("entryDetailsCard")).toBeTruthy()
        );
        expect(screen.getByTestId("backBtn")).toBeTruthy();
        expect(screen.getByText("Comirnaty")).toBeTruthy();
        // Back (web: history.back → popstate) closes it
        await press(screen.getByTestId("backBtn"));
        await waitFor(() =>
            expect(useTimelineStore.getState().fullscreenEntry).toBeUndefined()
        );
    });

    it("medications and immunizations together, newest first", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        jest.spyOn(
            services.medication,
            "getPatientMedicationStatements"
        ).mockResolvedValue(requestResult(medicationStatementsFixture()));
        await render(
            <Timeline
                hdid="HDID1"
                entryTypes={[EntryType.Immunization, EntryType.Medication]}
            />
        );
        expect(screen.getAllByTestId("entryCardDate").map(textOf)).toEqual([
            "2024-Mar-01",
            "2024-Feb-10",
            "2023-Oct-15",
            "2021-Mar-01",
        ]);
        expect(screen.getAllByTestId("medicationTitle")).toHaveLength(2);
    });

    it("lab results join the timeline; queued results show the processing alert", async () => {
        jest.spyOn(
            services.laboratory,
            "getLaboratoryOrders"
        ).mockResolvedValue(
            requestResult({
                loaded: true,
                queued: true,
                retryin: 0,
                orders: laboratoryOrdersFixture(),
            })
        );
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.LabResult]} />
        );
        expect(
            screen.getByTestId("laboratory-orders-queued-alert-message")
        ).toBeTruthy();
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe(
            "Lipid panel"
        );
    });

    it("health visits only (filter): the billing-claim alert", async () => {
        jest.spyOn(
            services.encounter,
            "getPatientEncounters"
        ).mockResolvedValue(requestResult(encountersFixture()));
        updateStores(() =>
            useTimelineStore
                .getState()
                .setFilter(
                    TimelineFilterBuilder.create().withEntryType(
                        EntryType.HealthVisit
                    )
                )
        );
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.HealthVisit]} />
        );
        expect(screen.getByTestId("timeline-health-visit-alert")).toBeTruthy();
        expect(screen.getAllByTestId("healthvisitTitle")).toHaveLength(2);
    });

    it("filter: counts per type, Apply sets the filter, a label's close clears just that part, Clear All resets", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        jest.spyOn(
            services.medication,
            "getPatientMedicationStatements"
        ).mockResolvedValue(requestResult(medicationStatementsFixture()));
        await render(
            <Timeline
                hdid="HDID1"
                entryTypes={[EntryType.Immunization, EntryType.Medication]}
            />
        );
        await waitFor(() =>
            expect(screen.getByTestId("filterDropdown")).toBeTruthy()
        );
        expect(screen.queryAllByTestId("filter-label")).toHaveLength(0);
        expect(screen.queryByTestId("clear-filters-button")).toBeNull();

        await press(screen.getByTestId("filterDropdown"));
        expect(
            textOf(screen.getByTestId(`${EntryType.Immunization}Count`))
        ).toBe(`(${immunizationResultFixture.immunizations.length})`);
        expect(textOf(screen.getByTestId(`${EntryType.Medication}Count`))).toBe(
            `(${medicationStatementsFixture().length})`
        );
        await changeText(screen.getByTestId("filterTextInput"), "Comirnaty");
        await press(screen.getByTestId(`${EntryType.Immunization}-filter`));
        await press(screen.getByTestId("btnFilterApply"));
        expect(screen.getAllByTestId("filter-label").map(textOf)).toEqual([
            '"Comirnaty"',
            "Immunizations",
        ]);
        const { filter } = useTimelineStore.getState();
        expect(filter.keyword).toBe("Comirnaty");
        expect([...filter.entryTypes]).toEqual([EntryType.Immunization]);

        // the keyword label's close leaves the type
        await press(screen.getAllByLabelText("Close")[0]);
        expect(screen.getAllByTestId("filter-label").map(textOf)).toEqual([
            "Immunizations",
        ]);
        expect(useTimelineStore.getState().filter.keyword).toBe("");

        await press(screen.getByTestId("clear-filters-button"));
        expect(screen.queryAllByTestId("filter-label")).toHaveLength(0);
        expect(useTimelineStore.getState().filter.hasActiveFilter()).toBe(
            false
        );
        await unmount();
    });

    it("filter: the date label reads From … To …, or either end alone", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        const labelFor = async (start: string, end: string) => {
            useTimelineStore
                .getState()
                .setFilter(
                    TimelineFilterBuilder.create()
                        .withStartDate(start)
                        .withEndDate(end)
                );
            await render(
                <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
            );
            await waitFor(() =>
                expect(screen.getByTestId("filter-label")).toBeTruthy()
            );
            const label = textOf(screen.getByTestId("filter-label"));
            await unmount();
            return label;
        };
        expect(await labelFor("2021-01-01", "2021-12-31")).toBe(
            "From 2021-Jan-01 To 2021-Dec-31"
        );
        expect(await labelFor("2021-01-01", "")).toBe("From 2021-Jan-01");
        expect(await labelFor("", "2021-12-31")).toBe("To 2021-Dec-31");
    });

    it("web: the record count is a status region, so a page or filter change is announced", async () => {
        if (Platform.OS !== "web") return;
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        const count = screen.getByTestId(
            "timeline-record-count"
        ) as unknown as HTMLElement;
        const region = count.closest('[role="status"]');
        expect(region).not.toBeNull();
        expect(region?.getAttribute("aria-live")).toBe("polite");
    });

    it("has hidden Filters and Records h2s under the page title, so the outline never skips a level", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        await render(
            <Timeline hdid="HDID1" entryTypes={[EntryType.Immunization]} />
        );
        // the entry cards' titles are buttons, not headings: the component's own outline is its two h2s
        const levels = headingLevels();
        expect(levels).toEqual([2, 2]);
        levels.forEach((level, i) => {
            if (i > 0) expect(level).toBeLessThanOrEqual(levels[i - 1] + 1);
        });
    });
});
