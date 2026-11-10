import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import type {
    BcCancerScreening,
    DiagnosticImagingExam,
} from "@/models/patientData";
import { BcCancerScreeningTimelineEntry } from "@/models/timeline/bcCancerScreeningTimelineEntry";
import { DiagnosticImagingTimelineEntry } from "@/models/timeline/diagnosticImagingTimelineEntry";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    patientDataFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { attrOf, press, render, screen, textOf, waitFor } from "@/test/ui";
import { saveBase64File } from "@/utils/saveFile";
import { BcCancerScreeningEntry } from "@/views/private/timeline/entries/BcCancerScreeningEntry";
import { DiagnosticImagingEntry } from "@/views/private/timeline/entries/DiagnosticImagingEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/saveFile", () => ({
    saveBase64File: jest.fn(async () => undefined),
}));

const [exam, result, recall] = patientDataFixture();

describe("imaging and BC Cancer entries (Diagnostic Imaging / BC Cancer Screening timeline components)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        jest.spyOn(services.patientData, "getFile").mockResolvedValue({
            content: [77, 97, 110],
            contentType: "application/pdf",
        });
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
        jest.clearAllMocks();
    });

    it("imaging: 'X-Ray - Recently Updated', status, description, authority; the report downloads as diagnostic_image_<date>.pdf", async () => {
        const entry = new DiagnosticImagingTimelineEntry(
            exam as DiagnosticImagingExam
        );
        expect(entry.title).toBe("X-Ray - Recently Updated");
        await render(<DiagnosticImagingEntry entry={entry} hdid="HDID1" />);
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Status: Final"
        );
        await press(screen.getByLabelText(/X-Ray/));
        expect(
            textOf(
                screen.getByTestId("diagnostic-imaging-procedure-description")
            )
        ).toBe("Description: Chest X-ray");
        expect(
            textOf(screen.getByTestId("diagnostic-imaging-health-authority"))
        ).toBe("Health Authority: VCH");
        await press(screen.getByTestId("diagnostic-imaging-download-button"));
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(saveBase64File).toHaveBeenCalledWith(
                "TWFu",
                "application/pdf",
                "diagnostic_image_2024_06_10-00_00.pdf"
            )
        );
    });

    it("BC Cancer result letter: titles, program subtitle, the result body with its link, and the bc_cancer_result download", async () => {
        const entry = new BcCancerScreeningTimelineEntry(
            result as BcCancerScreening
        );
        expect(entry.title).toBe("Breast Screening Result Letter");
        expect(entry.bcCancerDestination).toBe("Breast Cancer Screening");
        await render(<BcCancerScreeningEntry entry={entry} hdid="HDID1" />);
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Program: Breast Screening"
        );
        await press(screen.getByLabelText(/Breast Screening Result Letter/));
        expect(textOf(screen.getByTestId("bc-cancer-result-body"))).toContain(
            "Download your breast cancer screening result letter."
        );
        const link = screen.getByText("check the BC Cancer website");
        if (Platform.OS === "web")
            expect(attrOf(link, "href")).toBe(
                "http://www.bccancer.bc.ca/screening/breast"
            );
        await press(screen.getByTestId("bc-cancer-screening-download-button"));
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(saveBase64File).toHaveBeenCalledWith(
                "TWFu",
                "application/pdf",
                "bc_cancer_result_2024_07_05-09_00.pdf"
            )
        );
    });

    it("BC Cancer recall letter: the reminder title and the screening body ('cervix cancer')", async () => {
        const entry = new BcCancerScreeningTimelineEntry(
            recall as BcCancerScreening
        );
        expect(entry.title).toBe("Cervix Screening Reminder Letter");
        await render(<BcCancerScreeningEntry entry={entry} hdid="HDID1" />);
        await press(screen.getByLabelText(/Cervix Screening Reminder Letter/));
        expect(
            textOf(screen.getByTestId("bc-cancer-screening-body"))
        ).toContain("Learn more about cervix cancer screening");
    });
});
