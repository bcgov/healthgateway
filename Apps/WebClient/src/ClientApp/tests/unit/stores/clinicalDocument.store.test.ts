import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { ClinicalDocumentService } from "@/services/clinicalDocument.service";
import { services } from "@/services/container";
import {
    selectClinicalDocuments,
    useClinicalDocumentFileStore,
    useClinicalDocumentStore,
} from "@/stores/clinicalDocument.store";
import { useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import {
    configFixture,
    fakeAuth,
    FakeHttp,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const doc = {
    id: "d1",
    fileId: "f1",
    name: "Discharge summary",
    type: "Summary",
    facilityName: "VGH",
    discipline: "Medicine",
    serviceDate: "2024-04-02",
};
const media = {
    data: "QUJD",
    encoding: "base64",
    mediaType: "application/pdf",
};

describe("ClinicalDocumentService (restClinicalDocumentService port)", () => {
    it("GETs ClinicalDocument/{hdid} and ClinicalDocument/{hdid}/file/{fileId}; ClientApp-CI-CDO on failure", async () => {
        const config = {
            ...configFixture,
            serviceEndpoints: {
                ...configFixture.serviceEndpoints,
                ClinicalDocument: "https://api.test/cd/",
            },
        };
        useConfigStore.setState({
            config: {
                ...config,
                webClient: {
                    ...config.webClient,
                    featureToggleConfiguration: {
                        ...config.webClient.featureToggleConfiguration,
                        datasets: [{ name: "ClinicalDocument", enabled: true }],
                    },
                },
            },
            status: "loaded",
        });
        const http = new FakeHttp()
            .reply(requestResult([doc]))
            .reply(requestResult(media))
            .fail(new HttpError("x", 500));
        const svc = new ClinicalDocumentService(http, config);
        await svc.getRecords("H1");
        await svc.getFile("f1", "H1");
        expect(http.calls.map((c) => c.url)).toEqual([
            "https://api.test/cd/ClinicalDocument/H1",
            "https://api.test/cd/ClinicalDocument/H1/file/f1",
        ]);
        await expect(svc.getRecords("H1")).rejects.toMatchObject({
            errorCode: "ClientApp-CI-CDO",
        });
    });
});

describe("clinical document stores (stores/clinicalDocument.ts port)", () => {
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

    it("documents load per hdid", async () => {
        jest.spyOn(services.clinicalDocument, "getRecords").mockResolvedValue(
            requestResult([doc])
        );
        await useClinicalDocumentStore.getState().retrieve("H1");
        expect(
            selectClinicalDocuments("H1")(useClinicalDocumentStore.getState())
        ).toEqual([doc]);
    });

    it("files are fetched once; a failure banners 'Unable to download clinical document'", async () => {
        const get = jest
            .spyOn(services.clinicalDocument, "getFile")
            .mockResolvedValueOnce(requestResult(media))
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-CDO", "x", 500, "t")
            );
        await expect(
            useClinicalDocumentFileStore.getState().getFile("f1", "H1")
        ).resolves.toEqual(media);
        await useClinicalDocumentFileStore.getState().getFile("f1", "H1");
        expect(get).toHaveBeenCalledTimes(1);
        await expect(
            useClinicalDocumentFileStore.getState().getFile("f2", "H1")
        ).rejects.toThrow();
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to download clinical document"
        );
    });
});
