import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ResultError } from "@/models/errors";
import { LabResultTimelineEntry } from "@/models/timeline/labResultTimelineEntry";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    laboratoryOrdersFixture,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf, waitFor } from "@/test/ui";
import { downloadEncodedMedia } from "@/utils/encodedMedia";
import { LabResultEntry } from "@/views/private/timeline/entries/LabResultEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/encodedMedia", () => ({
    ...jest.requireActual<object>("@/utils/encodedMedia"),
    downloadEncodedMedia: jest.fn(async () => undefined),
}));

describe("LabResultEntry (LabResultTimelineComponent.vue port)", () => {
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
        jest.clearAllMocks();
    });

    const render1 = async () => {
        await render(
            <LabResultEntry
                entry={new LabResultTimelineEntry(laboratoryOrdersFixture()[0])}
                hdid="HDID1"
            />
        );
        await press(screen.getByLabelText(/Lab Results/));
    };

    it("shows the order, the tests table and the resource links", async () => {
        await render1();
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Order Status: Completed"
        );
        expect(textOf(screen.getByTestId("laboratory-collection-date"))).toBe(
            "Collection Date: 2024-May-01, 9:30 AM"
        );
        expect(textOf(screen.getByTestId("laboratory-reporting-lab"))).toBe(
            "Reporting Lab: LifeLabs"
        );
        expect(screen.getByText("BC CDC - Smart Sex Resource")).toBeTruthy();
        expect(screen.getByTestId("laboratoryResultTable")).toBeTruthy();
        expect(
            screen.getAllByTestId("laboratory-test-status-info-button")
        ).toHaveLength(3);
        expect(screen.getByTestId("attachment-button")).toBeTruthy();
    });

    it("the status tooltip opens with the status text", async () => {
        await render1();
        await press(
            screen.getAllByTestId("laboratory-test-status-info-button")[0]
        );
        await waitFor(() =>
            expect(
                screen.getByTestId(
                    "laboratory-test-status-info-lab1-0-0-popover"
                )
            ).toBeTruthy()
        );
        expect(
            screen.getByText("Download the PDF to see your detailed results.")
        ).toBeTruthy();
    });

    it("Download report → confirm → saves Laboratory_Report_<date>.pdf; failures banner", async () => {
        const get = jest
            .spyOn(services.laboratory, "getReportDocument")
            .mockResolvedValueOnce(
                requestResult({
                    data: "QUJD",
                    encoding: "base64",
                    mediaType: "application/pdf",
                })
            )
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-LAB", "x", 500, "t2")
            );
        await render1();
        await press(screen.getByTestId("laboratory-report-download-btn"));
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() => expect(downloadEncodedMedia).toHaveBeenCalled());
        expect(get).toHaveBeenCalledWith("lab1", "HDID1", false);
        expect(downloadEncodedMedia).toHaveBeenCalledWith(
            { data: "QUJD", encoding: "base64", mediaType: "application/pdf" },
            "Laboratory_Report_2024_05_01-09_30.pdf"
        );
        await press(screen.getByTestId("attachment-button"));
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(useErrorStore.getState().errors[0]?.title).toBe(
                "Unable to download laboratory report"
            )
        );
    });
});
