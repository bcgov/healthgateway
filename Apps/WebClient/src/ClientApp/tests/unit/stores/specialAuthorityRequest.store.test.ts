import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { LoadStatus } from "@/models/datasetState";
import { HttpError } from "@/models/errors";
import { services } from "@/services/container";
import { SpecialAuthorityService } from "@/services/specialAuthority.service";
import { useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import {
    selectSpecialAuthorityRequests,
    useSpecialAuthorityRequestStore,
} from "@/stores/specialAuthorityRequest.store";
import {
    configFixture,
    fakeAuth,
    FakeHttp,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const request = {
    drugName: "OZEMPIC",
    requestStatus: "Approved",
    requestedDate: "2024-01-10",
    prescriberFirstName: "Ada",
    prescriberLastName: "Lovelace",
    effectiveDate: "2024-01-15",
    expiryDate: "2025-01-15",
    referenceNumber: "SA-1",
};

describe("SpecialAuthorityService (restSpecialAuthorityService port)", () => {
    it("GETs MedicationRequest/{hdid} from the SpecialAuthority endpoint; ClientApp-CI-SPA on failure", async () => {
        const config = {
            ...configFixture,
            serviceEndpoints: {
                ...configFixture.serviceEndpoints,
                SpecialAuthority: "https://api.test/sa/",
            },
        };
        useConfigStore.setState({
            config: {
                ...config,
                webClient: {
                    ...config.webClient,
                    featureToggleConfiguration: {
                        ...config.webClient.featureToggleConfiguration,
                        datasets: [
                            { name: "SpecialAuthorityRequest", enabled: true },
                        ],
                    },
                },
            },
            status: "loaded",
        });
        const http = new FakeHttp()
            .reply(requestResult([request]))
            .fail(new HttpError("x", 500));
        const svc = new SpecialAuthorityService(http, config);
        await svc.getPatientMedicationRequest("H1");
        expect(http.calls[0].url).toBe(
            "https://api.test/sa/MedicationRequest/H1"
        );
        await expect(
            svc.getPatientMedicationRequest("H1")
        ).rejects.toMatchObject({ errorCode: "ClientApp-CI-SPA" });
    });
});

describe("Special Authority store (stores/specialAuthorityRequest.ts port)", () => {
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

    it("stores the requests", async () => {
        jest.spyOn(
            services.specialAuthority,
            "getPatientMedicationRequest"
        ).mockResolvedValue(requestResult([request]));
        await useSpecialAuthorityRequestStore
            .getState()
            .retrieveSpecialAuthorityRequests("H1");
        expect(
            selectSpecialAuthorityRequests("H1")(
                useSpecialAuthorityRequestStore.getState()
            )
        ).toHaveLength(1);
    });

    it("an error result throws and banners; an action-required result is kept as an error state without a banner", async () => {
        jest.spyOn(services.specialAuthority, "getPatientMedicationRequest")
            .mockResolvedValueOnce(
                requestResult([], 0, {
                    errorCode: "E",
                    traceId: "t",
                    resultMessage: "bad",
                })
            )
            .mockResolvedValueOnce(
                requestResult([], 2, {
                    errorCode: "A",
                    traceId: "t2",
                    resultMessage: "action",
                })
            );
        await expect(
            useSpecialAuthorityRequestStore
                .getState()
                .retrieveSpecialAuthorityRequests("H1")
        ).rejects.toThrow("bad");
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to retrieve special authorities"
        );
        await useSpecialAuthorityRequestStore
            .getState()
            .retrieveSpecialAuthorityRequests("H1");
        expect(
            useSpecialAuthorityRequestStore.getState().states.H1.status
        ).toBe(LoadStatus.ERROR);
        expect(useErrorStore.getState().errors).toHaveLength(1);
    });
});
