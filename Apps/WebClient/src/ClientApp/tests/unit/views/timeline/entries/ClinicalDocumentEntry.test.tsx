import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ClinicalDocumentTimelineEntry } from "@/models/timeline/clinicalDocumentTimelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen, textOf, waitFor } from "@/test/ui";
import { downloadEncodedMedia } from "@/utils/encodedMedia";
import { ClinicalDocumentEntry } from "@/views/timeline/entries/ClinicalDocumentEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/encodedMedia", () => ({
    ...jest.requireActual<object>("@/utils/encodedMedia"),
    downloadEncodedMedia: jest.fn(async () => undefined),
}));

const doc = {
    id: "d1",
    fileId: "f1",
    name: "Discharge summary",
    type: "Summary",
    facilityName: "VGH",
    discipline: "Medicine",
    serviceDate: "2024-04-02",
};

describe("ClinicalDocumentEntry (ClinicalDocumentTimelineComponent.vue port)", () => {
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

    it("name and type in the header; discipline and facility; Download Full Report saves Clinical_Document_<date>.pdf", async () => {
        const entry = new ClinicalDocumentTimelineEntry(doc);
        expect(
            entry.filterApplies(
                TimelineFilterBuilder.create().withKeyword("vgh").build()
            )
        ).toBe(true);
        jest.spyOn(services.clinicalDocument, "getFile").mockResolvedValue(
            requestResult({
                data: "QUJD",
                encoding: "base64",
                mediaType: "application/pdf",
            })
        );
        await render(<ClinicalDocumentEntry entry={entry} hdid="HDID1" />);
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe(
            "Discharge summary"
        );
        expect(screen.getByTestId("attachment-button")).toBeTruthy();
        await press(screen.getByLabelText(/Discharge summary/));
        expect(textOf(screen.getByTestId("clinical-document-discipline"))).toBe(
            "Type: Medicine"
        );
        expect(textOf(screen.getByTestId("clinical-document-facility"))).toBe(
            "Facility: VGH"
        );
        await press(screen.getByTestId("clinical-document-download-button"));
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(downloadEncodedMedia).toHaveBeenCalledWith(
                {
                    data: "QUJD",
                    encoding: "base64",
                    mediaType: "application/pdf",
                },
                "Clinical_Document_2024_04_02-00_00.pdf"
            )
        );
    });
});
