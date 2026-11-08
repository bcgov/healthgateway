import {
    afterAll,
    afterEach,
    beforeAll,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Settings } from "luxon";
import { Platform } from "react-native";

import { HealthVisitTimelineEntry } from "@/models/timeline/healthVisitTimelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    encountersFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf } from "@/test/ui";
import { HealthVisitEntry } from "@/views/timeline/entries/HealthVisitEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

// the roll-off warning depends on today: the fixture's "recent" visit turns six years old in 2031
const realNow = Settings.now;
beforeAll(() => {
    Settings.now = () => new Date("2026-10-31T12:00:00Z").getTime();
});
afterAll(() => {
    Settings.now = realNow;
});

describe("HealthVisitTimelineEntry (healthVisitTimelineEntry.ts port)", () => {
    it("unknown practitioner fallback, roll-off warning for visits six or more years old, keyword search", () => {
        const [recent, old] = encountersFixture().map(
            (e) => new HealthVisitTimelineEntry(e)
        );
        expect(recent.showRollOffWarning).toBe(false);
        expect(old.showRollOffWarning).toBe(true);
        expect(old.practitionerName).toBe("Unknown Practitioner");
        expect(
            recent.filterApplies(
                TimelineFilterBuilder.create().withKeyword("downtown").build()
            )
        ).toBe(true);
    });
});

describe("HealthVisitEntry (HealthVisitTimelineComponent.vue port)", () => {
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

    it("specialty as title, practitioner as subtitle; the clinic info toggles; no roll-off alert for recent visits", async () => {
        await render(
            <HealthVisitEntry
                entry={new HealthVisitTimelineEntry(encountersFixture()[0])}
                hdid="HDID1"
            />
        );
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe(
            "General Practice"
        );
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Dr Who"
        );
        await press(screen.getByLabelText(/Health Visits/));
        expect(textOf(screen.getByTestId("encounterClinicName"))).toBe(
            "Clinic/Practitioner: Downtown Clinic"
        );
        // v-show: native skips hidden views; web keeps it in the DOM with display none
        if (Platform.OS !== "web")
            expect(
                screen.queryByTestId("health-visit-clinic-name-info-popover")
            ).toBeNull();
        await press(screen.getByTestId("health-visit-clinic-info-button"));
        expect(
            screen.getByTestId("health-visit-clinic-name-info-popover")
        ).toBeTruthy();
        expect(screen.queryByTestId("encounterRolloffAlert")).toBeNull();
    });

    it("an old visit shows the roll-off warning", async () => {
        await render(
            <HealthVisitEntry
                entry={new HealthVisitTimelineEntry(encountersFixture()[1])}
                hdid="HDID1"
            />
        );
        await press(screen.getByLabelText(/Health Visits/));
        expect(screen.getByTestId("encounterRolloffAlert")).toBeTruthy();
    });
});
