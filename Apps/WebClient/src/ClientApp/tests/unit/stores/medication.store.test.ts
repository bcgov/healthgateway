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
    selectMedications,
    selectMedicationsAreProtected,
    selectProtectiveWordAttempts,
    useMedicationStore,
} from "@/stores/medication.store";
import {
    configFixture,
    fakeAuth,
    medicationStatementsFixture,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

describe("medication store (stores/medication.ts port)", () => {
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

    it("stores the statements with DIN / PIN padded to 8 digits, then answers from the store", async () => {
        const get = jest
            .spyOn(services.medication, "getPatientMedicationStatements")
            .mockResolvedValue(requestResult(medicationStatementsFixture()));
        await useMedicationStore.getState().retrieveMedications("H1");
        const meds = selectMedications("H1")(useMedicationStore.getState());
        expect(meds[0].medicationSummary.din).toBe("02240684");
        expect(meds[1].medicationSummary.din).toBe("66128150");
        expect(services.tracking.trackEvent).toHaveBeenCalled();
        const again = await useMedicationStore
            .getState()
            .retrieveMedications("H1");
        expect(get).toHaveBeenCalledTimes(1);
        expect(again.resourcePayload).toHaveLength(2);
    });

    it("PROTECTED: counts attempts and retries with the protective word", async () => {
        const protectedResult = requestResult([], 2, {
            errorCode: "x",
            traceId: "t",
            actionCode: ActionType.Protected,
        });
        const get = jest
            .spyOn(services.medication, "getPatientMedicationStatements")
            .mockResolvedValueOnce(protectedResult)
            .mockResolvedValueOnce(protectedResult)
            .mockResolvedValueOnce(
                requestResult(medicationStatementsFixture())
            );
        await useMedicationStore.getState().retrieveMedications("H1");
        expect(
            selectMedicationsAreProtected("H1")(useMedicationStore.getState())
        ).toBe(true);
        expect(
            selectProtectiveWordAttempts("H1")(useMedicationStore.getState())
        ).toBe(1);
        await useMedicationStore.getState().retrieveMedications("H1", "wrong");
        expect(
            selectProtectiveWordAttempts("H1")(useMedicationStore.getState())
        ).toBe(2);
        await useMedicationStore.getState().retrieveMedications("H1", "right");
        expect(get).toHaveBeenLastCalledWith("H1", "right");
        expect(useMedicationStore.getState().states.H1.status).toBe(
            LoadStatus.LOADED
        );
        expect(
            selectProtectiveWordAttempts("H1")(useMedicationStore.getState())
        ).toBe(0);
    });

    it("an API error result throws, sets the error state and adds the retrieve banner", async () => {
        jest.spyOn(
            services.medication,
            "getPatientMedicationStatements"
        ).mockResolvedValue(
            requestResult([], 0, {
                errorCode: "E1",
                traceId: "t9",
                resultMessage: "nope",
            })
        );
        await expect(
            useMedicationStore.getState().retrieveMedications("H1")
        ).rejects.toThrow("nope");
        expect(useMedicationStore.getState().states.H1.status).toBe(
            LoadStatus.ERROR
        );
        expect(useErrorStore.getState().errors[0]).toEqual({
            title: "Unable to retrieve medications",
            source: "medication",
            traceId: "t9",
        });
    });
});
