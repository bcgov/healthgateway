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
import { useErrorStore } from "@/stores/error.store";
import {
    selectLabResults,
    selectLabResultsAreQueued,
    useLabResultStore,
} from "@/stores/labResult.store";
import {
    configFixture,
    fakeAuth,
    laboratoryOrdersFixture,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

describe("lab result store (stores/labResult.ts port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        jest.useRealTimers();
        resetStores();
        jest.restoreAllMocks();
    });

    it("stores loaded orders with the queued flag", async () => {
        jest.spyOn(
            services.laboratory,
            "getLaboratoryOrders"
        ).mockResolvedValue(
            requestResult({
                loaded: true,
                queued: true,
                retryin: 0,
                orders: laboratoryOrdersFixture(),
            })
        );
        await useLabResultStore.getState().retrieveLabResults("H1");
        expect(
            selectLabResults("H1")(useLabResultStore.getState())
        ).toHaveLength(1);
        expect(
            selectLabResultsAreQueued("H1")(useLabResultStore.getState())
        ).toBe(true);
        expect(services.tracking.trackEvent).toHaveBeenCalled();
    });

    it("REFRESH: keeps the partial orders as loading and re-queries after retryin", async () => {
        jest.useFakeTimers();
        const get = jest
            .spyOn(services.laboratory, "getLaboratoryOrders")
            .mockResolvedValueOnce(
                requestResult(
                    { loaded: false, queued: false, retryin: 5000, orders: [] },
                    2,
                    {
                        errorCode: "x",
                        traceId: "t",
                        actionCode: ActionType.Refresh,
                    }
                )
            )
            .mockResolvedValueOnce(
                requestResult({
                    loaded: true,
                    queued: false,
                    retryin: 0,
                    orders: laboratoryOrdersFixture(),
                })
            );
        await useLabResultStore.getState().retrieveLabResults("H1");
        expect(useLabResultStore.getState().states.H1.status).toBe(
            LoadStatus.REQUESTED
        );
        await jest.advanceTimersByTimeAsync(5000);
        expect(get).toHaveBeenCalledTimes(2);
        expect(useLabResultStore.getState().states.H1.status).toBe(
            LoadStatus.LOADED
        );
    });

    it("an error result throws and banners 'Unable to retrieve laboratory results'", async () => {
        jest.spyOn(
            services.laboratory,
            "getLaboratoryOrders"
        ).mockResolvedValue(
            requestResult(
                { loaded: false, queued: false, retryin: 0, orders: [] },
                0,
                { errorCode: "E", traceId: "t1", resultMessage: "bad" }
            )
        );
        await expect(
            useLabResultStore.getState().retrieveLabResults("H1")
        ).rejects.toThrow("bad");
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to retrieve laboratory results"
        );
    });
});
