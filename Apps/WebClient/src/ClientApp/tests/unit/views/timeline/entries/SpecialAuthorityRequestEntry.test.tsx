import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { SpecialAuthorityRequestTimelineEntry } from "@/models/timeline/specialAuthorityRequestTimelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf } from "@/test/ui";
import { SpecialAuthorityRequestEntry } from "@/views/timeline/entries/SpecialAuthorityRequestEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const model = {
    drugName: "OZEMPIC",
    requestStatus: "Approved",
    requestedDate: "2024-01-10",
    prescriberFirstName: "Ada",
    prescriberLastName: "Lovelace",
    effectiveDate: "2024-01-15",
    referenceNumber: "SA-1",
};

describe("SpecialAuthorityRequestEntry (SpecialAuthorityRequestTimelineComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("drug name and status in the header; prescriber, reference and dates (yyyy-MM-dd, N/A when missing) in the details", async () => {
        const entry = new SpecialAuthorityRequestTimelineEntry(model);
        expect(
            entry.filterApplies(
                TimelineFilterBuilder.create().withKeyword("lovelace").build()
            )
        ).toBe(true);
        await render(
            <SpecialAuthorityRequestEntry entry={entry} hdid="HDID1" />
        );
        expect(
            textOf(screen.getByTestId("specialauthorityrequestTitle"))
        ).toContain("2024-Jan-10");
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Status: Approved"
        );
        expect(screen.queryByTestId("medicationPractitioner")).toBeNull();
        await press(screen.getByLabelText(/OZEMPIC/));
        expect(textOf(screen.getByTestId("medicationPractitioner"))).toBe(
            "Prescriber Name: Ada Lovelace"
        );
        expect(screen.getByText("2024-01-15")).toBeTruthy();
        expect(screen.getByText("N/A")).toBeTruthy();
    });
});
