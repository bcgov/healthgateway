import { describe, expect, it, jest } from "@jest/globals";

import { ResultType } from "@/constants/resultType";
import { HttpError, ResultError } from "@/models/errors";
import { ReportFormatType, TemplateType } from "@/models/report";
import { ImmunizationService } from "@/services/immunization.service";
import { ReportService } from "@/services/report.service";
import { useConfigStore } from "@/stores/config.store";
import {
    configFixture,
    FakeHttp,
    immunizationResultFixture,
} from "@/test/fixtures";

describe("ImmunizationService (restImmunizationService port)", () => {
    const withDatasets = (enabled: boolean) =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...configFixture.webClient.featureToggleConfiguration,
                        datasets: [{ name: "Immunization", enabled }],
                    },
                },
            },
            status: "loaded",
        });

    it("GETs Immunization?hdid=…&api-version=2.0 from the Immunization endpoint", async () => {
        withDatasets(true);
        const http = new FakeHttp().reply(immunizationResultFixture);
        const result = await new ImmunizationService(
            http,
            configFixture
        ).getPatientImmunizations("H1");
        expect(http.calls[0]).toMatchObject({
            method: "GET",
            url: "https://api.test/immunization/Immunization?hdid=H1&api-version=2.0",
        });
        expect(result).toBe(immunizationResultFixture);
    });

    it("returns empty lists without calling the API when the dataset is off", async () => {
        withDatasets(false);
        const http = new FakeHttp();
        await expect(
            new ImmunizationService(
                http,
                configFixture
            ).getPatientImmunizations("H1")
        ).resolves.toEqual({ immunizations: [], recommendations: [] });
        expect(http.calls).toHaveLength(0);
    });

    describe("an environment still on V1", () => {
        const v1Url = "https://api.test/immunization/Immunization?hdid=H1";
        const v1 = (refreshInProgress = false) => ({
            resourcePayload: {
                ...immunizationResultFixture,
                loadState: { refreshInProgress },
            },
            resultStatus: ResultType.Success,
            totalResultCount: 1,
            pageIndex: 0,
            pageSize: 0,
        });
        const service = (http: FakeHttp) =>
            new ImmunizationService(http, configFixture);

        it("falls back when V2 is refused with a 400, and unwraps the result", async () => {
            withDatasets(true);
            const http = new FakeHttp()
                .fail(new HttpError("unsupported version", 400))
                .reply(v1());
            await expect(
                service(http).getPatientImmunizations("H1")
            ).resolves.toEqual({
                immunizations: immunizationResultFixture.immunizations,
                recommendations: immunizationResultFixture.recommendations,
            });
            expect(http.calls[1].url).toBe(v1Url);
        });

        it("goes straight to V1 after finding it once", async () => {
            withDatasets(true);
            const http = new FakeHttp()
                .fail(new HttpError("unsupported version", 400))
                .reply(v1())
                .reply(v1());
            const immunizations = service(http);
            await immunizations.getPatientImmunizations("H1");
            await immunizations.getPatientImmunizations("H1");
            expect(http.calls.map((c) => c.url)).toEqual([
                `${v1Url}&api-version=2.0`,
                v1Url,
                v1Url,
            ]);
        });

        it("asks again while V1 says a refresh is running", async () => {
            jest.useFakeTimers();
            withDatasets(true);
            const http = new FakeHttp()
                .fail(new HttpError("unsupported version", 400))
                .reply(v1(true))
                .reply(v1(false));
            const result = service(http).getPatientImmunizations("H1");
            await jest.advanceTimersByTimeAsync(10000);
            await expect(result).resolves.toMatchObject({
                immunizations: immunizationResultFixture.immunizations,
            });
            expect(http.calls).toHaveLength(3);
            jest.useRealTimers();
        });

        it("stops asking eventually, with what it has", async () => {
            jest.useFakeTimers();
            withDatasets(true);
            const http = new FakeHttp().fail(new HttpError("no", 400));
            for (let i = 0; i < 12; i++) http.reply(v1(true));
            const result = service(http).getPatientImmunizations("H1");
            await jest.advanceTimersByTimeAsync(120000);
            await expect(result).resolves.toMatchObject({
                immunizations: immunizationResultFixture.immunizations,
            });
            expect(http.calls).toHaveLength(13);
            jest.useRealTimers();
        });

        it("reports a V1 error result", async () => {
            withDatasets(true);
            const http = new FakeHttp()
                .fail(new HttpError("unsupported version", 400))
                .reply({
                    ...v1(),
                    resultStatus: ResultType.Error,
                    resultError: {
                        errorCode: "X",
                        resultMessage: "bad",
                        statusCode: 500,
                    },
                });
            await expect(
                service(http).getPatientImmunizations("H1")
            ).rejects.toMatchObject({ errorCode: "X" });
        });

        it("a 400 from V1 as well is reported as the failure it is", async () => {
            withDatasets(true);
            const http = new FakeHttp()
                .fail(new HttpError("bad", 400))
                .fail(new HttpError("bad", 400));
            await expect(
                service(http).getPatientImmunizations("H1")
            ).rejects.toMatchObject({
                errorCode: "ClientApp-CI-IMZ",
                statusCode: 400,
            });
        });

        it("does not fall back on any other failure", async () => {
            withDatasets(true);
            const http = new FakeHttp().fail(new HttpError("down", 503));
            await expect(
                service(http).getPatientImmunizations("H1")
            ).rejects.toBeInstanceOf(ResultError);
            expect(http.calls).toHaveLength(1);
        });
    });

    it("translates failures to ResultError ClientApp-CI-IMZ", async () => {
        withDatasets(true);
        const http = new FakeHttp().fail(new HttpError("boom", 500));
        const error = await new ImmunizationService(http, configFixture)
            .getPatientImmunizations("H1")
            .catch((e: unknown) => e);
        expect(error).toBeInstanceOf(ResultError);
        expect(error).toMatchObject({
            errorCode: "ClientApp-CI-IMZ",
            statusCode: 500,
        });
    });
});

describe("ReportService (restReportService port)", () => {
    it("POSTs the report request to GatewayApi Report", async () => {
        const http = new FakeHttp().reply({
            resourcePayload: { fileName: "r.pdf", data: "QUJD" },
        });
        const request = {
            data: { header: {}, records: [] },
            template: TemplateType.ImmunizationRecommendation,
            type: ReportFormatType.PDF,
        };
        const result = await new ReportService(
            http,
            configFixture
        ).generateReport(request);
        expect(http.calls[0]).toMatchObject({
            method: "POST",
            url: "https://api.test/gateway/Report",
            body: request,
        });
        expect(result.resourcePayload.fileName).toBe("r.pdf");
    });

    it("translates failures to ResultError ClientApp-CI-REP", async () => {
        const http = new FakeHttp().fail(new HttpError("busy", 429));
        await expect(
            new ReportService(http, configFixture).generateReport({
                data: {},
                template: TemplateType.Immunization,
                type: ReportFormatType.CSV,
            })
        ).rejects.toMatchObject({
            errorCode: "ClientApp-CI-REP",
            statusCode: 429,
        });
    });
});
