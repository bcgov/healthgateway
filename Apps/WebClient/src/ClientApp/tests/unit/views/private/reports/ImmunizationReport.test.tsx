import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { createRef } from "react";

import { AppText } from "@/components/common";
import { ReportFormatType, TemplateType } from "@/models/report";
import { ReportFilterBuilder } from "@/models/reportFilter";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { render, screen, textOf, waitFor } from "@/test/ui";
import {
    ImmunizationReport,
    type ImmunizationReportHandle,
} from "@/views/private/reports/ImmunizationReport";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const filter = ReportFilterBuilder.buildEmpty();

describe("ImmunizationReport (ImmunizationReportComponent.vue port)", () => {
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

    it("loads the hdid's immunizations and shows both tables, newest first", async () => {
        const get = jest
            .spyOn(services.immunization, "getPatientImmunizations")
            .mockResolvedValue(immunizationResultFixture);
        await render(<ImmunizationReport hdid="HDID1" filter={filter} />);
        expect(get).toHaveBeenCalledWith("HDID1");
        expect(screen.getByText("Immunization Record")).toBeTruthy();
        expect(
            screen.getAllByTestId("immunizationDateItem").map(textOf)
        ).toEqual(["2023-Oct-15", "2021-Mar-01"]);
        expect(screen.getByText("Recommended Immunizations")).toBeTruthy();
        expect(screen.getAllByTestId("recommendationItem").map(textOf)).toEqual(
            ["Shingles", "Tetanus"]
        );
        expect(
            screen.getAllByTestId("recommendationDateItem").map(textOf)
        ).toEqual(["2027-Jun-30", "2025-Jan-01"]);
        expect(screen.getByText("Comirnaty")).toBeTruthy();
    });

    it("hideImmunizations + hideRecommendationHeader + a description slot (the dialog's use)", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        await render(
            <ImmunizationReport
                hdid="HDID1"
                filter={filter}
                forceShow
                hideImmunizations
                hideRecommendationHeader
                recommendationsDescription={<AppText>Custom text</AppText>}
            />
        );
        expect(screen.queryByText("Immunization Record")).toBeNull();
        expect(screen.queryByText("Recommended Immunizations")).toBeNull();
        expect(screen.getByText("Custom text")).toBeTruthy();
        expect(
            screen.getByTestId("recommendation-history-report-table")
        ).toBeTruthy();
    });

    it("no data: 'No records found.' and reports empty", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue({ immunizations: [], recommendations: [] });
        const onIsEmptyChanged = jest.fn();
        await render(
            <ImmunizationReport
                hdid="HDID1"
                filter={filter}
                onIsEmptyChanged={onIsEmptyChanged}
            />
        );
        expect(screen.getByText("No records found.")).toBeTruthy();
        expect(onIsEmptyChanged).toHaveBeenLastCalledWith(true);
    });

    it("hidden on mobile unless forceShow (d-none d-md-block)", async () => {
        setBreakpoint(390);
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        await render(<ImmunizationReport hdid="HDID1" filter={filter} />);
        expect(screen.queryByText("Immunization Record")).toBeNull();
    });

    it("generateReport posts the rows, recommendations and header with the template", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
        const generate = jest
            .spyOn(services.report, "generateReport")
            .mockResolvedValue({
                resourcePayload: { fileName: "f.pdf", data: "x" },
                totalResultCount: 1,
                pageIndex: 0,
                pageSize: 1,
                resultStatus: 1,
            });
        const ref = createRef<ImmunizationReportHandle>();
        await render(
            <ImmunizationReport
                ref={ref}
                hdid="HDID1"
                filter={filter}
                template={TemplateType.ImmunizationRecommendation}
            />
        );
        await waitFor(() => expect(ref.current).toBeTruthy());
        const header = {
            phn: "1",
            name: "A B",
            dateOfBirth: "",
            datePrinted: "",
            filterText: "",
        };
        await ref.current?.generateReport(ReportFormatType.CSV, header);
        expect(generate).toHaveBeenCalledWith({
            data: {
                header,
                records: [
                    expect.objectContaining({ date: "2023-Oct-15" }),
                    expect.objectContaining({
                        date: "2021-Mar-01",
                        immunization: "COVID-19 mRNA",
                        provider_clinic: "Pharmacy A",
                    }),
                ],
                recommendations: [
                    { immunization: "Shingles", due_date: "2027-Jun-30" },
                    { immunization: "Tetanus", due_date: "2025-Jan-01" },
                ],
            },
            template: TemplateType.ImmunizationRecommendation,
            type: ReportFormatType.CSV,
        });
    });
});
