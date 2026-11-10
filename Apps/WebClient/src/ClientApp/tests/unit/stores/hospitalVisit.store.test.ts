import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { HospitalVisitService } from "@/services/hospitalVisit.service";
import { useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import {
    selectHospitalVisits,
    useHospitalVisitStore,
} from "@/stores/hospitalVisit.store";
import {
    configFixture,
    fakeAuth,
    FakeHttp,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const visit = {
    encounterId: "h1",
    facility: "VGH",
    healthService: "Emergency",
    visitType: "Outpatient Visit",
    healthAuthority: "VCH",
    admitDateTime: "2024-02-01T08:30:00",
    endDateTime: "2024-02-01T12:00:00",
    provider: "Dr X",
};

describe("HospitalVisitService (restHospitalVisitService port)", () => {
    it("GETs Encounter/HospitalVisit/{hdid}?api-version=2.0; ClientApp-CI-HOS on failure", async () => {
        const config = {
            ...configFixture,
            serviceEndpoints: {
                ...configFixture.serviceEndpoints,
                HospitalVisit: "https://api.test/hv/",
            },
        };
        useConfigStore.setState({
            config: {
                ...config,
                webClient: {
                    ...config.webClient,
                    featureToggleConfiguration: {
                        ...config.webClient.featureToggleConfiguration,
                        datasets: [{ name: "HospitalVisit", enabled: true }],
                    },
                },
            },
            status: "loaded",
        });
        const http = new FakeHttp()
            .reply([visit])
            .fail(new HttpError("x", 500));
        const svc = new HospitalVisitService(http, config);
        await expect(svc.getHospitalVisits("H1")).resolves.toEqual([visit]);
        expect(http.calls[0].url).toBe(
            "https://api.test/hv/Encounter/HospitalVisit/H1?api-version=2.0"
        );
        await expect(svc.getHospitalVisits("H1")).rejects.toMatchObject({
            errorCode: "ClientApp-CI-HOS",
        });
    });
});

describe("hospital visit store (stores/hospitalVisit.ts port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("stores the visits; a failure banners", async () => {
        jest.spyOn(services.hospitalVisit, "getHospitalVisits")
            .mockResolvedValueOnce([visit])
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-HOS", "x", 500, "t")
            );
        await useHospitalVisitStore.getState().retrieveHospitalVisits("H1");
        expect(
            selectHospitalVisits("H1")(useHospitalVisitStore.getState())
        ).toEqual([visit]);
        await expect(
            useHospitalVisitStore.getState().retrieveHospitalVisits("H2")
        ).rejects.toThrow();
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to retrieve hospital visits"
        );
    });
});
