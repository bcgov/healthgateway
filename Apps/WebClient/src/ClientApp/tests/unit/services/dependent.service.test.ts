import { describe, expect, it } from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { DependentService } from "@/services/dependent.service";
import { configFixture, FakeHttp, requestResult } from "@/test/fixtures";

const dependent = {
    ownerId: "OWNER",
    delegateId: "ME",
    totalDelegateCount: 1,
    reasonCode: 0,
    version: 1,
    dependentInformation: {
        hdid: "OWNER",
        PHN: "9874307168",
        firstname: "Sam",
        lastname: "Testfive",
        dateOfBirth: "2025-03-15",
    },
};
const request = {
    firstName: "Sam",
    lastName: "Testfive",
    dateOfBirth: "2025-03-15",
    PHN: "9874307168",
};
const disabledConfig = {
    ...configFixture,
    webClient: {
        ...configFixture.webClient,
        featureToggleConfiguration: {
            ...configFixture.webClient.featureToggleConfiguration,
            dependents: {
                enabled: false,
                timelineEnabled: false,
                datasets: [],
            },
        },
    },
};

describe("DependentService (restDependentService port)", () => {
    it("uses GatewayApi UserProfile/{hdid}/Dependent for add, list and remove", async () => {
        const http = new FakeHttp()
            .reply(requestResult(request))
            .reply(requestResult([dependent]))
            .reply(requestResult(undefined));
        const svc = new DependentService(http, configFixture);
        await expect(svc.addDependent("ME", request)).resolves.toEqual(request);
        await expect(svc.getAll("ME")).resolves.toEqual([dependent]);
        await svc.removeDependent("ME", dependent);
        expect(http.calls.map((c) => [c.method, c.url])).toEqual([
            ["POST", "https://api.test/gateway/UserProfile/ME/Dependent"],
            ["GET", "https://api.test/gateway/UserProfile/ME/Dependent"],
            [
                "DELETE",
                "https://api.test/gateway/UserProfile/ME/Dependent/OWNER",
            ],
        ]);
        expect(http.calls[0].body).toEqual(request);
    });

    it("does not add a dependent when the feature is off", async () => {
        const http = new FakeHttp();
        const svc = new DependentService(http, disabledConfig);
        await expect(svc.addDependent("ME", request)).resolves.toBeUndefined();
        expect(http.calls).toHaveLength(0);
    });

    it("translates transport errors and throws API errors with their action code", async () => {
        const http = new FakeHttp().reply(new HttpError("nope", 500)).reply(
            requestResult(undefined, 0, {
                resultMessage: "Mismatch",
                errorCode: "X",
                actionCode: "MISMATCH",
            } as never)
        );
        const svc = new DependentService(http, configFixture);
        await expect(svc.getAll("ME")).rejects.toBeInstanceOf(ResultError);
        await expect(svc.addDependent("ME", request)).rejects.toMatchObject({
            message: "Mismatch",
            actionCode: "MISMATCH",
        });
    });
});
