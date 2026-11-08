import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ActionType } from "@/constants/actionType";
import { LoadStatus } from "@/models/datasetState";
import { services } from "@/services/container";
import {
    selectCovid19TestResults,
    useCovid19TestResultStore,
} from "@/stores/covid19TestResult.store";
import { useErrorStore } from "@/stores/error.store";
import {
    configFixture,
    covid19OrdersFixture,
    fakeAuth,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

describe("COVID-19 test result store (stores/covid19TestResult.ts port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        jest.useRealTimers();
        resetStores();
        jest.restoreAllMocks();
    });

    it("stores loaded orders and answers from the store after", async () => {
        const get = jest
            .spyOn(services.laboratory, "getCovid19LaboratoryOrders")
            .mockResolvedValue(
                requestResult({
                    loaded: true,
                    retryin: 0,
                    orders: covid19OrdersFixture(),
                })
            );
        await useCovid19TestResultStore
            .getState()
            .retrieveCovid19TestResults("H1");
        await useCovid19TestResultStore
            .getState()
            .retrieveCovid19TestResults("H1");
        expect(
            selectCovid19TestResults("H1")(useCovid19TestResultStore.getState())
        ).toHaveLength(2);
        expect(get).toHaveBeenCalledTimes(1);
    });

    it("REFRESH: stays requested and re-queries after retryin", async () => {
        jest.useFakeTimers();
        const get = jest
            .spyOn(services.laboratory, "getCovid19LaboratoryOrders")
            .mockResolvedValueOnce(
                requestResult({ loaded: false, retryin: 3000, orders: [] }, 2, {
                    errorCode: "x",
                    traceId: "t",
                    actionCode: ActionType.Refresh,
                })
            )
            .mockResolvedValueOnce(
                requestResult({
                    loaded: true,
                    retryin: 0,
                    orders: covid19OrdersFixture(),
                })
            );
        await useCovid19TestResultStore
            .getState()
            .retrieveCovid19TestResults("H1");
        expect(useCovid19TestResultStore.getState().states.H1.status).toBe(
            LoadStatus.REQUESTED
        );
        await jest.advanceTimersByTimeAsync(3000);
        expect(get).toHaveBeenCalledTimes(2);
        expect(useCovid19TestResultStore.getState().states.H1.status).toBe(
            LoadStatus.LOADED
        );
    });

    it("an error result throws and banners as a laboratory result", async () => {
        jest.spyOn(
            services.laboratory,
            "getCovid19LaboratoryOrders"
        ).mockResolvedValue(
            requestResult({ loaded: false, retryin: 0, orders: [] }, 0, {
                errorCode: "E",
                traceId: "t",
                resultMessage: "bad",
            })
        );
        await expect(
            useCovid19TestResultStore
                .getState()
                .retrieveCovid19TestResults("H1")
        ).rejects.toThrow("bad");
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to retrieve laboratory results"
        );
    });
});
