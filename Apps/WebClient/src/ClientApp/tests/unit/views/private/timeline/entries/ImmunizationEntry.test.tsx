import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ImmunizationTimelineEntry } from "@/models/timeline/immunizationTimelineEntry";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf } from "@/test/ui";
import { ImmunizationEntry } from "@/views/private/timeline/entries/ImmunizationEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const validDose = new ImmunizationTimelineEntry({
    id: "v1",
    status: "Completed",
    dateOfImmunization: "2021-07-14T00:00:00",
    providerOrClinic: "Burnaby Hospital",
    immunization: {
        name: "COVID-19 mRNA",
        immunizationAgents: [
            {
                code: "a",
                name: "Agent",
                lotNumber: "L1",
                productName: "Comirnaty",
            },
        ],
    },
    forecast: {
        createDate: "2021-07-14",
        status: "Eligible",
        displayName: "COVID-19 Non-replicating Viral Vector",
        eligibleDate: "2021-08-01",
        dueDate: "2021-08-11",
    },
});
const invalidDose = new ImmunizationTimelineEntry({
    id: "x1",
    status: "Invalid",
    dateOfImmunization: "2021-03-30T00:00:00",
    providerOrClinic: "",
    immunization: {
        name: "",
        immunizationAgents: [
            { code: "b", name: "Agent", lotNumber: "", productName: "" },
        ],
    },
});

describe("ImmunizationEntry (ImmunizationTimelineComponent.vue port, ui/timeline/immunization.js)", () => {
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

    it("a valid dose shows product, agent, provider, lot and the Forecast (h3)", async () => {
        await render(
            <ImmunizationEntry
                entry={validDose}
                hdid="HDID1"
                testID="timelineCard"
            />
        );
        expect(textOf(screen.getByTestId("immunizationTitle"))).toContain(
            "2021-Jul-14"
        );
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe(
            "COVID-19 mRNA"
        );
        await press(screen.getByLabelText(/Immunizations/));
        for (const id of [
            "immunizationProductTitle",
            "immunizationAgentNameTitle",
            "immunizationProviderTitle",
            "immunizationLotTitle",
        ])
            expect(screen.getByTestId(id)).toBeTruthy();
        expect(textOf(screen.getByTestId("immunizationLotTitle"))).toBe(
            "Lot Number: L1"
        );
        expect(screen.getByText("Forecast")).toBeTruthy();
        expect(textOf(screen.getByTestId("forecastDisplayName"))).toContain(
            "COVID-19 Non-replicating Viral Vector"
        );
        expect(textOf(screen.getByTestId("forecastDueDate"))).toContain(
            "2021-08-11"
        );
        expect(services.tracking.trackEvent).toHaveBeenCalled();
    });

    it("an invalid dose with an empty name shows only the agent, no forecast, and no details title", async () => {
        await render(<ImmunizationEntry entry={invalidDose} hdid="HDID1" />);
        expect(textOf(screen.getByTestId("immunizationTitle"))).toContain(
            "Immunizations"
        );
        expect(screen.queryByTestId("entryCardDetailsTitle")).toBeNull();
        await press(screen.getByLabelText(/Immunizations/));
        expect(screen.getByTestId("immunizationAgentNameTitle")).toBeTruthy();
        for (const id of [
            "immunizationProductTitle",
            "immunizationProviderTitle",
            "immunizationLotTitle",
            "forecastDisplayName",
        ])
            expect(screen.queryByTestId(id)).toBeNull();
        expect(screen.queryByText("Forecast")).toBeNull();
    });

    it("pressing again hides the details", async () => {
        await render(<ImmunizationEntry entry={validDose} hdid="HDID1" />);
        await press(screen.getByLabelText(/Immunizations/));
        await press(screen.getByLabelText(/Immunizations/));
        expect(screen.queryByTestId("immunizationProductTitle")).toBeNull();
    });
});
