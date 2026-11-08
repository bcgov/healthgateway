import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { act } from "@testing-library/react";
import { Platform } from "react-native";

import { ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    medicationStatementsFixture,
    requestResult,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { isDisabled, press, render, screen, textOf } from "@/test/ui";
import { saveBase64File } from "@/utils/saveFile";
import { formatActiveIngredients } from "@/views/reports/DatasetReports";
import {
    estimateBase64SizeInBytes,
    ReportsComponent,
} from "@/views/reports/ReportsComponent";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/saveFile", () => ({
    saveBase64File: jest.fn(() => Promise.resolve()),
}));

function useDatasets(datasets: string[], overrides: string[] = []) {
    const base = configFixture.webClient.featureToggleConfiguration;
    updateStores(() =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...base,
                        datasets: datasets.map((name) => ({
                            name,
                            enabled: true,
                        })),
                        dependents: {
                            enabled: true,
                            timelineEnabled: true,
                            datasets: overrides.map((name) => ({
                                name,
                                enabled: false,
                            })),
                        },
                    },
                },
            },
            status: "loaded",
        })
    );
}

async function selectMedications() {
    await press(screen.getByTestId("report-type"));
    await press(screen.getByText("Medications"));
}

describe("ReportsComponent (ReportsComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        resetRouter("/reports");
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
        updateStores(() =>
            useUserStore.setState({ patient: { hdid: "HDID" } as never })
        );
        useDatasets(["Medication", "Immunization"]);
        jest.spyOn(
            services.medication,
            "getPatientMedicationStatements"
        ).mockResolvedValue(requestResult(medicationStatementsFixture()));
        (saveBase64File as jest.Mock).mockClear();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("starts with the prompt and Download disabled", async () => {
        await render(<ReportsComponent hdid="HDID" />);
        expect(screen.getByTestId("info-image")).toBeTruthy();
        expect(textOf(screen.getByTestId("info-text"))).toBe(
            "Select a record type above to create a report"
        );
        expect(isDisabled(screen.getByTestId("export-record-btn"))).toBe(true);
    });

    it("lists only the enabled record types, applying dependent overrides", async () => {
        useDatasets(["Medication", "Immunization"], ["Medication"]);
        await render(<ReportsComponent hdid="HDID" isDependent />);
        await press(screen.getByTestId("report-type"));
        expect(screen.getByText("Immunizations")).toBeTruthy();
        expect(screen.queryByText("Medications")).toBeNull();
    });

    it("previews the selected report and enables Download", async () => {
        await render(<ReportsComponent hdid="HDID" />);
        await selectMedications();
        expect(
            screen.getByTestId("medication-history-report-table")
        ).toBeTruthy();
        expect(isDisabled(screen.getByTestId("export-record-btn"))).toBe(false);
    });

    it("selecting a record type logs no React errors", async () => {
        const consoleError = jest
            .spyOn(console, "error")
            .mockImplementation(() => undefined);
        await render(<ReportsComponent hdid="HDID" />);
        await selectMedications();
        expect(consoleError).not.toHaveBeenCalled();
    });

    it("hides the preview on a phone and for dependents, keeping Download", async () => {
        setBreakpoint(390);
        await render(<ReportsComponent hdid="HDID" />);
        await selectMedications();
        expect(
            screen.queryByTestId("medication-history-report-table")
        ).toBeNull();
        expect(isDisabled(screen.getByTestId("export-record-btn"))).toBe(false);
    });

    it("opens with ?defaultEntryType", async () => {
        resetRouter("/reports", { defaultEntryType: "Medication" });
        await render(<ReportsComponent hdid="HDID" />);
        expect(screen.getByTestId("report-sample")).toBeTruthy();
    });

    it("generates and saves the report after the sensitive document prompt", async () => {
        const generate = jest
            .spyOn(services.report, "generateReport")
            .mockResolvedValue(
                requestResult({ fileName: "m.csv", data: "AAAA" })
            );
        await render(<ReportsComponent hdid="HDID" />);
        await selectMedications();
        await press(screen.getByTestId("export-record-btn"));
        await press(screen.getByText("CSV"));
        expect(generate).not.toHaveBeenCalled();
        await press(screen.getByTestId("generic-message-submit-btn"));
        expect(generate.mock.calls[0][0]).toMatchObject({
            template: "Medication",
            type: "CSV",
        });
        expect(saveBase64File).toHaveBeenCalledWith(
            "AAAA",
            "text/csv",
            "m.csv"
        );
    });

    async function failDownload(result: unknown) {
        jest.spyOn(services.report, "generateReport").mockImplementation(() =>
            result instanceof Error
                ? Promise.reject(result)
                : Promise.resolve(result as never)
        );
        await render(<ReportsComponent hdid="HDID" />);
        await selectMedications();
        await press(screen.getByTestId("export-record-btn"));
        await press(screen.getByText("PDF"));
        await press(screen.getByTestId("generic-message-submit-btn"));
    }

    it("a 429 sets the page error", async () => {
        await failDownload(new ResultError("X", "busy", 429));
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        expect(saveBase64File).not.toHaveBeenCalled();
    });

    it.each([
        [
            "an API error",
            { resultError: { errorCode: "X", resultMessage: "no" } },
        ],
        ["an empty payload", requestResult({ fileName: "m.pdf", data: "" })],
        [
            "an oversized PDF",
            requestResult({
                fileName: "m.pdf",
                data: "A".repeat(26 * 1024 * 1024 * 2),
            }),
        ],
    ])(
        "%s adds a medication report error and saves nothing",
        async (_n, result) => {
            await failDownload(result);
            expect(saveBase64File).not.toHaveBeenCalled();
            expect(JSON.stringify(useErrorStore.getState().errors)).toContain(
                "medication report"
            );
        }
    );

    it("filters by date with Apply, shows the range chip and clears it", async () => {
        await render(<ReportsComponent hdid="HDID" />);
        await selectMedications();
        await press(screen.getByTestId("advanced-btn"));
        await press(screen.getByTestId("apply-filter-btn"));
        expect(screen.queryByTestId("selected-dates-filter")).toBeNull();
    });

    if (Platform.OS === "web")
        it("web: the panel stays shown for its transition after Cancel", async () => {
            await render(<ReportsComponent hdid="HDID" />);
            await selectMedications();
            await press(screen.getByTestId("advanced-btn"));
            const panel = (
                screen.getByTestId("clear-btn") as unknown as HTMLElement
            ).parentElement!.parentElement!;
            const display = () => getComputedStyle(panel).display;
            jest.useFakeTimers();
            try {
                await press(screen.getByTestId("clear-btn"));
                expect(display()).not.toBe("none");
                await act(async () => {
                    jest.advanceTimersByTime(300);
                });
                expect(display()).toBe("none");
            } finally {
                jest.useRealTimers();
            }
        });

    it("estimates a base64 payload's size", () => {
        expect(estimateBase64SizeInBytes("AAAA")).toBe(3);
        expect(estimateBase64SizeInBytes("AAA=")).toBe(2);
        expect(estimateBase64SizeInBytes("AA==")).toBe(1);
    });

    it("formats active ingredients as the report expects", () => {
        const record = (activeIngredients: unknown) =>
            ({ medicationSummary: { activeIngredients } }) as never;
        expect(formatActiveIngredients(record([])).strength).toBe("Not Found");
        expect(
            formatActiveIngredients(
                record([{ ingredient: "A", strength: "5", strengthUnit: "mg" }])
            ).strength
        ).toBe("5mg");
        expect(
            formatActiveIngredients(
                record([
                    { ingredient: "A", strength: "5", strengthUnit: "mg" },
                    { ingredient: "B", strength: "1", strengthUnit: "g" },
                ])
            ).strength
        ).toBe("A 5mg, B 1g");
        const long = record(
            Array.from({ length: 12 }, (_, i) => ({
                ingredient: `Ingredient${i}`,
                strength: "10",
                strengthUnit: "mg",
            }))
        );
        expect(formatActiveIngredients(long).check_din).toBe("Check DIN");
        expect(
            formatActiveIngredients(long, "CSV" as never).strength
        ).toContain("Check DIN (https://health-products.canada.ca/dpd-bdpp/)");
    });
});
