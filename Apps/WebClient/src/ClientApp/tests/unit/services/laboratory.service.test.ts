import { describe, expect, it } from "@jest/globals";

import { HttpError } from "@/models/errors";
import { LaboratoryService } from "@/services/laboratory.service";
import { useConfigStore } from "@/stores/config.store";
import { configFixture, FakeHttp, requestResult } from "@/test/fixtures";

const config = {
    ...configFixture,
    serviceEndpoints: {
        ...configFixture.serviceEndpoints,
        Laboratory: "https://api.test/lab/",
    },
};
const withDatasets = (lab: boolean, covid: boolean) =>
    useConfigStore.setState({
        config: {
            ...config,
            webClient: {
                ...config.webClient,
                featureToggleConfiguration: {
                    ...config.webClient.featureToggleConfiguration,
                    datasets: [
                        { name: "LabResult", enabled: lab },
                        { name: "Covid19TestResult", enabled: covid },
                    ],
                },
            },
        },
        status: "loaded",
    });

describe("LaboratoryService (restLaboratoryService port)", () => {
    it("GETs lab orders, COVID-19 orders and report documents", async () => {
        withDatasets(true, true);
        const http = new FakeHttp()
            .reply(requestResult({}))
            .reply(requestResult({}))
            .reply(requestResult({}));
        const svc = new LaboratoryService(http, config);
        await svc.getLaboratoryOrders("H1");
        await svc.getCovid19LaboratoryOrders("H1");
        await svc.getReportDocument("r1", "H1", false);
        expect(http.calls.map((c) => c.url)).toEqual([
            "https://api.test/lab/Laboratory/LaboratoryOrders?hdid=H1",
            "https://api.test/lab/Laboratory/Covid19Orders?hdid=H1",
            "https://api.test/lab/Laboratory/r1/Report?hdid=H1&isCovid19=false",
        ]);
    });

    it("datasets off: empty results without calling the API", async () => {
        withDatasets(false, false);
        const http = new FakeHttp();
        const svc = new LaboratoryService(http, config);
        expect((await svc.getLaboratoryOrders("H1")).resourcePayload).toEqual({
            loaded: true,
            queued: false,
            retryin: 0,
            orders: [],
        });
        expect(
            (await svc.getCovid19LaboratoryOrders("H1")).resourcePayload
        ).toEqual({ loaded: true, retryin: 0, orders: [] });
        expect(
            (await svc.getReportDocument("r", "H1", true)).resourcePayload
        ).toEqual({ data: "", encoding: "", mediaType: "" });
        expect(http.calls).toHaveLength(0);
    });

    it("translates failures to ClientApp-CI-LAB", async () => {
        withDatasets(true, true);
        const http = new FakeHttp().fail(new HttpError("x", 500));
        await expect(
            new LaboratoryService(http, config).getLaboratoryOrders("H1")
        ).rejects.toMatchObject({
            errorCode: "ClientApp-CI-LAB",
        });
    });
});
