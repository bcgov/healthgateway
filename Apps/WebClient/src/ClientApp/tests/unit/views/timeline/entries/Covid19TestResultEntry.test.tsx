import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { Covid19TestResultTimelineEntry } from "@/models/timeline/covid19TestResultTimelineEntry";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    covid19OrdersFixture,
    fakeAuth,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { attrOf, press, render, screen, textOf, waitFor } from "@/test/ui";
import { downloadEncodedMedia } from "@/utils/encodedMedia";
import { Covid19TestResultEntry } from "@/views/timeline/entries/Covid19TestResultEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/encodedMedia", () => ({
    ...jest.requireActual<object>("@/utils/encodedMedia"),
    downloadEncodedMedia: jest.fn(async () => undefined),
}));

describe("Covid19TestResultEntry (Covid19TestResultTimelineComponent.vue port)", () => {
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

    it("one ready result: the outcome in the header, the details, the description link and the report download", async () => {
        jest.spyOn(services.laboratory, "getReportDocument").mockResolvedValue(
            requestResult({
                data: "QUJD",
                encoding: "base64",
                mediaType: "application/pdf",
            })
        );
        await render(
            <Covid19TestResultEntry
                entry={
                    new Covid19TestResultTimelineEntry(
                        covid19OrdersFixture()[0]
                    )
                }
                hdid="HDID1"
            />
        );
        expect(textOf(screen.getByTestId("laboratoryHeaderDescription"))).toBe(
            "Result: Negative"
        );
        await press(screen.getByLabelText(/COVID/));
        expect(textOf(screen.getByTestId("laboratoryReportingLab"))).toBe(
            "Reporting Lab: Provincial Lab"
        );
        expect(textOf(screen.getByTestId("laboratoryTestStatus-0"))).toBe(
            "Test Status: Final"
        );
        expect(screen.getByText(/^2021-Jan-06, 8:15\sAM$/)).toBeTruthy();
        const link = screen.getByTestId("result-link");
        if (Platform.OS === "web")
            expect(attrOf(link, "href")).toBe("https://www.bccdc.ca/covid");
        await press(screen.getByTestId("covid-result-download-btn"));
        await press(screen.getByTestId("generic-message-submit-btn"));
        await waitFor(() =>
            expect(downloadEncodedMedia).toHaveBeenCalledWith(
                {
                    data: "QUJD",
                    encoding: "base64",
                    mediaType: "application/pdf",
                },
                "COVID_Result_2021_01_05-10_00.pdf"
            )
        );
        expect(services.laboratory.getReportDocument).toHaveBeenCalledWith(
            "c1",
            "HDID1",
            true
        );
    });

    it("several tests: no header outcome, a Result per ready test, no download", async () => {
        await render(
            <Covid19TestResultEntry
                entry={
                    new Covid19TestResultTimelineEntry(
                        covid19OrdersFixture()[1]
                    )
                }
                hdid="HDID1"
            />
        );
        expect(screen.queryByTestId("laboratoryHeaderDescription")).toBeNull();
        await press(screen.getByLabelText(/COVID/));
        expect(
            screen.getAllByTestId("laboratoryTestResult").map(textOf)
        ).toEqual(["Result: Positive"]);
        expect(textOf(screen.getByTestId("laboratoryTestStatus-0"))).toBe(
            "Test Status: Pending"
        );
        expect(screen.queryByTestId("covid-result-download-btn")).toBeNull();
        expect(screen.queryByTestId("attachment-button")).toBeNull();
    });
});
