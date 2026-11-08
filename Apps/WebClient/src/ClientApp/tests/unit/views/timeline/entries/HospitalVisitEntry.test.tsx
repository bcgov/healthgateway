import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { HospitalVisitTimelineEntry } from "@/models/timeline/hospitalVisitTimelineEntry";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf } from "@/test/ui";
import { HospitalVisitEntry } from "@/views/timeline/entries/HospitalVisitEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const visit = (overrides = {}) => ({
    encounterId: "h1",
    facility: "VGH",
    healthService: "Emergency",
    visitType: "Outpatient Visit",
    healthAuthority: "VCH",
    admitDateTime: "2024-02-01T08:30:00",
    endDateTime: "",
    provider: "Dr X",
    ...overrides,
});

describe("HospitalVisitEntry (HospitalVisitTimelineComponent.vue port)", () => {
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

    it("an outpatient visit: facility and visit type; location, provider and date tips; N/A discharge", async () => {
        await render(
            <HospitalVisitEntry
                entry={new HospitalVisitTimelineEntry(visit())}
                hdid="HDID1"
            />
        );
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe("VGH");
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Outpatient Visit"
        );
        await press(screen.getByLabelText(/VGH/));
        expect(textOf(screen.getByTestId("hospital-visit-date"))).toContain(
            "Visit Date: 2024-Feb-01, 8:30"
        );
        expect(
            textOf(screen.getByTestId("hospital-visit-discharge-date"))
        ).toBe("Discharge Date: N/A");
        for (const id of [
            "hospital-visit-location",
            "hospital-visit-provider",
            "hospital-visit-date",
        ])
            expect(screen.getByTestId(`${id}-info-button`)).toBeTruthy();
    });

    it("an inpatient visit has no date tip", async () => {
        await render(
            <HospitalVisitEntry
                entry={
                    new HospitalVisitTimelineEntry(
                        visit({ visitType: "Inpatient" })
                    )
                }
                hdid="HDID1"
            />
        );
        await press(screen.getByLabelText(/VGH/));
        expect(
            screen.queryByTestId("hospital-visit-date-info-button")
        ).toBeNull();
    });
});
