import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { MedicationTimelineEntry } from "@/models/timeline/medicationTimelineEntry";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    medicationStatementsFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf } from "@/test/ui";
import { MedicationEntry } from "@/views/timeline/entries/MedicationEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("MedicationEntry (MedicationTimelineComponent.vue port)", () => {
    const [rx, assessment] = medicationStatementsFixture();
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

    it("a prescription: header, then practitioner, DIN, quantity, form, manufacturer, ingredients, pharmacy and directions", async () => {
        await render(
            <MedicationEntry
                entry={new MedicationTimelineEntry(rx)}
                hdid="HDID1"
            />
        );
        expect(textOf(screen.getByTestId("medicationTitle"))).toContain(
            "Medications"
        );
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe(
            "Amoxicillin"
        );
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "500 mg capsule"
        );
        await press(screen.getByLabelText(/Medications/));
        expect(textOf(screen.getByTestId("medication-practitioner"))).toBe(
            "Practitioner: SMITH"
        );
        expect(textOf(screen.getByTestId("medication-din-pin"))).toBe(
            "DIN: 2240684"
        );
        expect(textOf(screen.getByTestId("medication-quantity"))).toBe(
            "Quantity: 30"
        );
        expect(textOf(screen.getByTestId("medication-ingredient-0"))).toBe(
            "AMOXICILLIN"
        );
        expect(textOf(screen.getByTestId("medication-strength-0"))).toBe(
            "500MG"
        );
        expect(textOf(screen.getByTestId("medication-pharmacy-name"))).toBe(
            "Filled at: Main St Pharmacy"
        );
        expect(textOf(screen.getByTestId("medication-pharmacy-phone"))).toBe(
            "Phone Number: (250) 555-1234"
        );
        expect(textOf(screen.getByTestId("medication-pharmacy-fax"))).toBe(
            "Fax: (250) 555-9876"
        );
        expect(textOf(screen.getByTestId("medication-directions"))).toBe(
            "Directions for use: Take one daily"
        );
        expect(screen.queryByTestId("pharmacist-outcome")).toBeNull();
    });

    it("a pharmacist assessment: PIN, pharmacy, outcome and the redirect note", async () => {
        await render(
            <MedicationEntry
                entry={new MedicationTimelineEntry(assessment)}
                hdid="HDID1"
            />
        );
        await press(screen.getByLabelText(/Medications/));
        expect(textOf(screen.getByTestId("medication-din-pin"))).toBe(
            "PIN: 66128150"
        );
        expect(textOf(screen.getByTestId("medication-pharmacy-name"))).toBe(
            "Pharmacy: Corner Pharmacy"
        );
        expect(textOf(screen.getByTestId("pharmacist-outcome"))).toBe(
            "Outcome: Prescription provided"
        );
        expect(
            screen.getByTestId("pharmacist-redirected-to-provider")
        ).toBeTruthy();
        expect(screen.queryByTestId("medication-directions")).toBeNull();
        expect(screen.queryByTestId("medication-pharmacy-phone")).toBeNull();
    });
});
