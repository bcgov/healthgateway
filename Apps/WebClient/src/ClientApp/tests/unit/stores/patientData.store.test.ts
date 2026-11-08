import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { PatientDataType } from "@/models/patientData";
import { services } from "@/services/container";
import { PatientDataService } from "@/services/patientData.service";
import { useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import {
    patientDataRecords,
    usePatientDataStore,
} from "@/stores/patientData.store";
import {
    configFixture,
    fakeAuth,
    FakeHttp,
    patientDataFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { bytesToBase64 } from "@/utils/bytes";

const withDatasets = (enabled: boolean) =>
    useConfigStore.setState({
        config: {
            ...configFixture,
            serviceEndpoints: {
                ...configFixture.serviceEndpoints,
                PatientData: "https://api.test/pd/",
            },
            webClient: {
                ...configFixture.webClient,
                featureToggleConfiguration: {
                    ...configFixture.webClient.featureToggleConfiguration,
                    datasets: [
                        { name: "DiagnosticImaging", enabled },
                        { name: "BcCancerScreening", enabled },
                    ],
                },
            },
        },
        status: "loaded",
    });

describe("PatientDataService (restPatientDataService port)", () => {
    const config = {
        ...configFixture,
        serviceEndpoints: {
            ...configFixture.serviceEndpoints,
            PatientData: "https://api.test/pd/",
        },
    };

    it("GETs PatientData/{hdid} with one patientDataTypes per type; files from /file/{id}", async () => {
        withDatasets(true);
        const http = new FakeHttp()
            .reply({ items: [] })
            .reply({ content: [], contentType: "application/pdf" });
        const svc = new PatientDataService(http, config);
        await svc.getPatientData("H1", [
            PatientDataType.DiagnosticImaging,
            PatientDataType.BcCancerScreening,
        ]);
        await svc.getFile("H1", "f1");
        expect(http.calls.map((c) => c.url)).toEqual([
            "https://api.test/pd/PatientData/H1?patientDataTypes=DiagnosticImaging&patientDataTypes=BcCancerScreening&api-version=2.0",
            "https://api.test/pd/PatientData/H1/file/f1?api-version=2.0",
        ]);
    });

    it("a disabled dataset is refused without calling the API; transport failures are ClientApp-CI-PATD", async () => {
        withDatasets(false);
        const http = new FakeHttp().fail(new HttpError("x", 500));
        const svc = new PatientDataService(http, config);
        await expect(
            svc.getPatientData("H1", [PatientDataType.DiagnosticImaging])
        ).rejects.toThrow("Dataset DiagnosticImaging is not enabled");
        expect(http.calls).toHaveLength(0);
        await expect(svc.getFile("H1", "f")).rejects.toMatchObject({
            errorCode: "ClientApp-CI-PATD",
        });
    });
});

describe("patient data store (stores/patientData.ts port)", () => {
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

    it("splits the items by type, and answers from the store only when every type is stored", async () => {
        const get = jest
            .spyOn(services.patientData, "getPatientData")
            .mockResolvedValue({ items: patientDataFixture() });
        const types = [
            PatientDataType.DiagnosticImaging,
            PatientDataType.BcCancerScreening,
        ];
        await usePatientDataStore.getState().retrievePatientData("H1", types);
        const state = usePatientDataStore.getState().states.H1;
        expect(
            patientDataRecords(state, [PatientDataType.DiagnosticImaging])
        ).toHaveLength(1);
        expect(
            patientDataRecords(state, [PatientDataType.BcCancerScreening])
        ).toHaveLength(2);
        await usePatientDataStore.getState().retrievePatientData("H1", types);
        expect(get).toHaveBeenCalledTimes(1);
        await usePatientDataStore
            .getState()
            .retrievePatientData("H1", [
                PatientDataType.OrganDonorRegistrationStatus,
            ])
            .catch(() => undefined);
        expect(get).toHaveBeenCalledTimes(2);
    });

    it("a failure banners once per requested type", async () => {
        jest.spyOn(services.patientData, "getPatientData").mockRejectedValue(
            new ResultError("ClientApp-CI-PATD", "x", 500)
        );
        await expect(
            usePatientDataStore
                .getState()
                .retrievePatientData("H1", [
                    PatientDataType.DiagnosticImaging,
                    PatientDataType.BcCancerScreening,
                ])
        ).rejects.toThrow();
        expect(useErrorStore.getState().errors.map((e) => e.title)).toEqual([
            "Unable to retrieve diagnostic imaging exams",
            "Unable to retrieve BC Cancer screenings",
        ]);
    });

    it("files are fetched once", async () => {
        const get = jest
            .spyOn(services.patientData, "getFile")
            .mockResolvedValue({
                content: [1, 2, 3],
                contentType: "application/pdf",
            });
        await usePatientDataStore
            .getState()
            .retrievePatientDataFile("H1", "f1");
        await usePatientDataStore
            .getState()
            .retrievePatientDataFile("H1", "f1");
        expect(get).toHaveBeenCalledTimes(1);
    });
});

describe("bytesToBase64", () => {
    it.each([
        [[], ""],
        [[77], "TQ=="],
        [[77, 97], "TWE="],
        [[77, 97, 110], "TWFu"],
        [[0, 255, 128, 1], "AP+AAQ=="],
    ])("%j → %s", (bytes, base64) => {
        expect(bytesToBase64(bytes)).toBe(base64);
    });
});
