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
import { ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import {
    selectImmunizations,
    selectImmunizationsAreLoading,
    selectRecommendations,
    useImmunizationStore,
} from "@/stores/immunization.store";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

describe("immunization store (stores/immunization.ts port)", () => {
    let track: jest.SpiedFunction<typeof services.tracking.trackEvent>;
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        track = jest
            .spyOn(services.tracking, "trackEvent")
            .mockImplementation(() => undefined);
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("loads per hdid, keeps recommendations with vaccinations sorted by due date (latest first), and tracks the load", async () => {
        const get = jest
            .spyOn(services.immunization, "getPatientImmunizations")
            .mockResolvedValue(immunizationResultFixture);
        const pending = useImmunizationStore
            .getState()
            .retrieveImmunizations("H1");
        expect(
            selectImmunizationsAreLoading("H1")(useImmunizationStore.getState())
        ).toBe(true);
        await pending;
        const state = useImmunizationStore.getState();
        expect(get).toHaveBeenCalledWith("H1");
        expect(selectImmunizations("H1")(state)).toHaveLength(2);
        expect(
            selectRecommendations("H1")(state).map(
                (r) => r.recommendedVaccinations
            )
        ).toEqual(["Shingles", "Tetanus"]);
        expect(state.states.H1.status).toBe(LoadStatus.LOADED);
        expect(track).toHaveBeenCalledWith({
            action: Action.Load,
            text: Text.Data,
            dataset: Dataset.Immunizations,
        });
        // another hdid is untouched
        expect(selectImmunizations("H2")(state)).toEqual([]);
    });

    it("does not query again once loaded", async () => {
        const get = jest
            .spyOn(services.immunization, "getPatientImmunizations")
            .mockResolvedValue(immunizationResultFixture);
        await useImmunizationStore.getState().retrieveImmunizations("H1");
        await useImmunizationStore.getState().retrieveImmunizations("H1");
        expect(get).toHaveBeenCalledTimes(1);
    });

    it("no tracking event when there are no immunizations", async () => {
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue({ immunizations: [], recommendations: [] });
        await useImmunizationStore.getState().retrieveImmunizations("H1");
        expect(track).not.toHaveBeenCalled();
    });

    it("a failure sets the error state, adds a retrieve banner and rethrows; a 429 sets the page warning", async () => {
        jest.spyOn(services.immunization, "getPatientImmunizations")
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-IMZ", "boom", 500, "t1")
            )
            .mockRejectedValueOnce(
                new ResultError("ClientApp-CI-IMZ", "busy", 429)
            );
        await expect(
            useImmunizationStore.getState().retrieveImmunizations("H1")
        ).rejects.toThrow("boom");
        expect(useImmunizationStore.getState().states.H1.status).toBe(
            LoadStatus.ERROR
        );
        expect(useErrorStore.getState().errors[0]).toEqual({
            title: "Unable to retrieve immunizations",
            source: "immunization",
            traceId: "t1",
        });
        await expect(
            useImmunizationStore.getState().retrieveImmunizations("H1")
        ).rejects.toThrow("busy");
        expect(useErrorStore.getState().tooManyRequestsWarning).toBe("page");
    });
});
