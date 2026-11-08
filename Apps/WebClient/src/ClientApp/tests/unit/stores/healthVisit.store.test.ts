import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { Action, Dataset, Text } from "@/constants/tracking";
import { LoadStatus } from "@/models/datasetState";
import { HttpError, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { EncounterService } from "@/services/encounter.service";
import { useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import {
    selectHealthVisits,
    selectHealthVisitsAreLoading,
    useHealthVisitStore,
} from "@/stores/healthVisit.store";
import {
    configFixture,
    encountersFixture,
    fakeAuth,
    FakeHttp,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

describe("EncounterService (restEncounterService port)", () => {
    const config = {
        ...configFixture,
        serviceEndpoints: {
            ...configFixture.serviceEndpoints,
            Encounter: "https://api.test/enc/",
        },
    };
    const withVisits = (enabled: boolean) =>
        useConfigStore.setState({
            config: {
                ...config,
                webClient: {
                    ...config.webClient,
                    featureToggleConfiguration: {
                        ...config.webClient.featureToggleConfiguration,
                        datasets: [{ name: "HealthVisit", enabled }],
                    },
                },
            },
            status: "loaded",
        });

    it("GETs Encounter/{hdid}; empty when the dataset is off; ClientApp-CI-ENC on failure", async () => {
        withVisits(true);
        const http = new FakeHttp()
            .reply(requestResult([]))
            .fail(new HttpError("x", 500));
        const svc = new EncounterService(http, config);
        await svc.getPatientEncounters("H1");
        expect(http.calls[0].url).toBe("https://api.test/enc/Encounter/H1");
        await expect(svc.getPatientEncounters("H1")).rejects.toMatchObject({
            errorCode: "ClientApp-CI-ENC",
        });
        withVisits(false);
        expect((await svc.getPatientEncounters("H1")).resourcePayload).toEqual(
            []
        );
        expect(http.calls).toHaveLength(2);
    });
});

describe("health visit store (stores/healthVisit.ts port, createRequestResultStore)", () => {
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

    it("loads once, tracks a non-empty load, answers from the store after", async () => {
        const get = jest
            .spyOn(services.encounter, "getPatientEncounters")
            .mockResolvedValue(requestResult(encountersFixture()));
        const pending = useHealthVisitStore.getState().retrieve("H1");
        expect(
            selectHealthVisitsAreLoading("H1")(useHealthVisitStore.getState())
        ).toBe(true);
        await pending;
        expect(
            selectHealthVisits("H1")(useHealthVisitStore.getState())
        ).toHaveLength(2);
        expect(services.tracking.trackEvent).toHaveBeenCalledWith({
            action: Action.Load,
            text: Text.Data,
            dataset: Dataset.HealthVisits,
        });
        await useHealthVisitStore.getState().retrieve("H1");
        expect(get).toHaveBeenCalledTimes(1);
    });

    it("an error result throws a ResultError and banners; a 429 warns instead", async () => {
        jest.spyOn(services.encounter, "getPatientEncounters")
            .mockResolvedValueOnce(
                requestResult([], 0, {
                    errorCode: "E",
                    traceId: "t",
                    resultMessage: "bad",
                })
            )
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-ENC", "busy", 429)
            );
        await expect(
            useHealthVisitStore.getState().retrieve("H1")
        ).rejects.toThrow("bad");
        expect(useHealthVisitStore.getState().states.H1.status).toBe(
            LoadStatus.ERROR
        );
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to retrieve health visits"
        );
        await expect(
            useHealthVisitStore.getState().retrieve("H1")
        ).rejects.toThrow("busy");
        expect(useErrorStore.getState().tooManyRequestsWarning).toBe("page");
    });
});
