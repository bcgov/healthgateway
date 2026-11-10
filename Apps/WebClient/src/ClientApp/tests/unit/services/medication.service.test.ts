import { describe, expect, it } from "@jest/globals";

import { HttpError } from "@/models/errors";
import { MedicationService } from "@/services/medication.service";
import { useConfigStore } from "@/stores/config.store";
import { configFixture, FakeHttp, requestResult } from "@/test/fixtures";

const withMedications = (enabled: boolean) =>
    useConfigStore.setState({
        config: {
            ...configFixture,
            serviceEndpoints: {
                ...configFixture.serviceEndpoints,
                Medication: "https://api.test/medication/",
            },
            webClient: {
                ...configFixture.webClient,
                featureToggleConfiguration: {
                    ...configFixture.webClient.featureToggleConfiguration,
                    datasets: [{ name: "Medication", enabled }],
                },
            },
        },
        status: "loaded",
    });

describe("MedicationService (restMedicationService port)", () => {
    const endpoints = {
        ...configFixture,
        serviceEndpoints: {
            ...configFixture.serviceEndpoints,
            Medication: "https://api.test/medication/",
        },
    };

    it("GETs MedicationStatement/{hdid}, with the protective word header when given", async () => {
        withMedications(true);
        const http = new FakeHttp()
            .reply(requestResult([]))
            .reply(requestResult([]));
        const svc = new MedicationService(http, endpoints);
        await svc.getPatientMedicationStatements("H1");
        await svc.getPatientMedicationStatements("H1", "secret");
        expect(http.calls[0]).toMatchObject({
            method: "GET",
            url: "https://api.test/medication/MedicationStatement/H1",
            headers: {},
        });
        expect(http.calls[1].headers).toEqual({ protectiveWord: "secret" });
    });

    it("an empty success without calling the API when the dataset is off", async () => {
        withMedications(false);
        const http = new FakeHttp();
        const result = await new MedicationService(
            http,
            endpoints
        ).getPatientMedicationStatements("H1");
        expect(result).toMatchObject({ resultStatus: 1, resourcePayload: [] });
        expect(http.calls).toHaveLength(0);
    });

    it("translates failures to ClientApp-CI-MED", async () => {
        withMedications(true);
        const http = new FakeHttp().fail(new HttpError("boom", 502));
        await expect(
            new MedicationService(
                http,
                endpoints
            ).getPatientMedicationStatements("H1")
        ).rejects.toMatchObject({
            errorCode: "ClientApp-CI-MED",
            statusCode: 502,
        });
    });
});
