import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import {
    Action,
    Actor,
    Dataset,
    Destination,
    Format,
    Origin,
    Text,
} from "@/constants/tracking";
import { ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { isDisabled, press, render, screen, waitFor } from "@/test/ui";
import { openExternal } from "@/utils/openExternal";
import { saveBase64File } from "@/utils/saveFile";
import { RecommendationsDialog } from "@/views/reports/RecommendationsDialog";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/saveFile", () => ({
    saveBase64File: jest.fn(async () => undefined),
}));
jest.mock("@/utils/openExternal", () => ({
    openExternal: jest.fn(async () => undefined),
}));

const reportResult = {
    resourcePayload: {
        fileName: "HealthGatewayImmunizations.pdf",
        data: "QUJD",
    },
    totalResultCount: 1,
    pageIndex: 0,
    pageSize: 1,
    resultStatus: 1,
};

async function openDialog(onClose = jest.fn()) {
    await render(
        <RecommendationsDialog visible hdid="HDID1" onClose={onClose} />
    );
    await waitFor(() =>
        expect(screen.getAllByTestId("recommendationItem")).toHaveLength(2)
    );
    return onClose;
}

async function chooseFormat(format: string) {
    await press(screen.getByTestId("export-recommendations-record-button"));
    await press(screen.getByText(format));
}

describe("RecommendationsDialog (RecommendationsDialogComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
        jest.clearAllMocks();
    });

    it("shows the recommendations table with the web copy; Close closes", async () => {
        const onClose = await openDialog();
        expect(screen.getByTestId("recommendations-dialog")).toBeTruthy();
        expect(screen.getByText("Recommended Immunizations")).toBeTruthy();
        expect(screen.getByText("BC Vaccine Schedule")).toBeTruthy();
        expect(screen.queryByText("Immunization Record")).toBeNull();
        await press(screen.getByTestId("close-recommendations-dialog-button"));
        expect(onClose).toHaveBeenCalledTimes(1);
        await press(screen.getByTestId("recommendations-dialog-close-button"));
        expect(onClose).toHaveBeenCalledTimes(2);
    });

    it("Book a COVID or Flu Vaccination opens the booking site and tracks it", async () => {
        await openDialog();
        await press(screen.getByTestId("book-vaccination-button"));
        expect(openExternal).toHaveBeenCalledWith(
            "https://www.getvaccinated.gov.bc.ca/s/"
        );
        expect(services.tracking.trackEvent).toHaveBeenCalledWith({
            action: Action.Visit,
            text: Text.ExternalLink,
            destination: Destination.BookVaccine,
            origin: Origin.ImmunizationRecommendationDialog,
        });
    });

    it("Download Record → format → Sensitive Document → Continue generates and saves the file", async () => {
        const generate = jest
            .spyOn(services.report, "generateReport")
            .mockResolvedValue(reportResult);
        await openDialog();
        expect(
            isDisabled(
                screen.getByTestId("export-recommendations-record-button")
            )
        ).toBe(false);
        await chooseFormat("PDF");
        expect(screen.getByText("Sensitive Document")).toBeTruthy();
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() => expect(saveBase64File).toHaveBeenCalled());
        expect(generate).toHaveBeenCalledWith(
            expect.objectContaining({
                template: "ImmunizationRecommendation",
                type: "PDF",
            })
        );
        expect(saveBase64File).toHaveBeenCalledWith(
            "QUJD",
            "application/pdf",
            "HealthGatewayImmunizations.pdf"
        );
        expect(services.tracking.trackEvent).toHaveBeenCalledWith({
            action: Action.Download,
            text: Text.Export,
            dataset: Dataset.Immunizations,
            format: Format.Pdf,
            actor: Actor.User,
        });
    });

    it("Cancel on the confirmation does not download", async () => {
        const generate = jest.spyOn(services.report, "generateReport");
        await openDialog();
        await chooseFormat("CSV");
        await press(screen.getByTestId("generic-message-ok-btn"));
        expect(generate).not.toHaveBeenCalled();
    });

    it("a 429 shows the too-many message in the dialog; other failures add a download banner", async () => {
        jest.spyOn(services.report, "generateReport")
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-REP", "busy", 429)
            )
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-REP", "boom", 500, "t7")
            );
        await openDialog();
        await chooseFormat("XLSX");
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(screen.getByTestId("too-many-requests-error")).toBeTruthy()
        );
        expect(useErrorStore.getState().tooManyRequestsError).toBe(
            "recommendationsDialog"
        );
        await chooseFormat("XLSX");
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(useErrorStore.getState().errors[0]).toEqual({
                title: "Unable to download export record",
                source: "export‑record",
                traceId: "t7",
            })
        );
    });

    it("Download Record is disabled with no recommendations", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue({ immunizations: [], recommendations: [] });
        await render(
            <RecommendationsDialog
                visible
                hdid="HDID1"
                onClose={() => undefined}
            />
        );
        expect(screen.getByText("No records found.")).toBeTruthy();
        expect(
            isDisabled(
                screen.getByTestId("export-recommendations-record-button")
            )
        ).toBe(true);
    });

    it("on mobile the dialog is full screen and still lists the recommendations", async () => {
        setBreakpoint(390);
        await openDialog();
        expect(screen.getByTestId("recommendations-dialog")).toBeTruthy();
        expect(
            screen.getByTestId("recommendation-history-report-table")
        ).toBeTruthy();
    });
});
