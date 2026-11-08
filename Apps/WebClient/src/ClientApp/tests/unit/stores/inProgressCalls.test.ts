import { afterEach, describe, expect, it, jest } from "@jest/globals";

import { PatientDataType } from "@/models/patientData";
import { services } from "@/services/container";
import { useCommentStore } from "@/stores/comment.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import { useMedicationStore } from "@/stores/medication.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import { inProgressCalls } from "@/stores/utils/inProgressCalls";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
} from "@/test/fixtures";

const deferred = <T>() => {
    let resolve: (value: T) => void = () => undefined;
    let reject: (error: Error) => void = () => undefined;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
};

describe("inProgressCalls", () => {
    it("hands a second request the running call, and starts again once it ended", async () => {
        const calls = inProgressCalls();
        const first = deferred<number>();
        const start = jest.fn(() => first.promise);
        const a = calls.share("k", start);
        const b = calls.share("k", start);
        expect(start).toHaveBeenCalledTimes(1);
        first.resolve(1);
        await expect(a).resolves.toBe(1);
        await expect(b).resolves.toBe(1);
        const again = jest.fn(async () => 2);
        await expect(calls.share("k", again)).resolves.toBe(2);
        expect(again).toHaveBeenCalledTimes(1);
    });

    it("a failed call fails everyone waiting, and is not kept", async () => {
        const calls = inProgressCalls();
        const first = deferred<number>();
        const a = calls.share("k", () => first.promise);
        const b = calls.share("k", () => first.promise);
        first.reject(new Error("no"));
        await expect(a).rejects.toThrow("no");
        await expect(b).rejects.toThrow("no");
        expect(calls.running("k")).toBeUndefined();
    });

    it("keeps calls for different keys apart, and never shares without a key", () => {
        const calls = inProgressCalls();
        const start = jest.fn(() => new Promise<number>(() => undefined));
        void calls.share("a", start);
        void calls.share("b", start);
        void calls.share(undefined, start);
        void calls.share(undefined, start);
        expect(start).toHaveBeenCalledTimes(4);
    });
});

describe("stores share a fetch in progress", () => {
    afterEach(() => {
        jest.restoreAllMocks();
        resetStores();
    });
    const start = () => services.initialize(configFixture, fakeAuth());

    it("immunizations: one call for two requests, per person", async () => {
        start();
        const call = deferred<typeof immunizationResultFixture>();
        const get = jest
            .spyOn(services.immunization, "getPatientImmunizations")
            .mockReturnValue(call.promise);
        const store = useImmunizationStore.getState();
        const one = store.retrieveImmunizations("A");
        const two = store.retrieveImmunizations("A");
        void store.retrieveImmunizations("B").catch(() => undefined);
        expect(get).toHaveBeenCalledTimes(2);
        call.resolve(immunizationResultFixture);
        await Promise.all([one, two]);
    });

    it("medications: a protective word always makes its own call", () => {
        start();
        const get = jest
            .spyOn(services.medication, "getPatientMedicationStatements")
            .mockReturnValue(new Promise(() => undefined) as never);
        const store = useMedicationStore.getState();
        void store.retrieveMedications("A");
        void store.retrieveMedications("A");
        expect(get).toHaveBeenCalledTimes(1);
        void store.retrieveMedications("A", "word");
        expect(get).toHaveBeenCalledTimes(2);
        expect(get).toHaveBeenLastCalledWith("A", "word");
    });

    it("comments: one call for two requests", () => {
        start();
        const get = jest
            .spyOn(services.userComment, "getCommentsForProfile")
            .mockReturnValue(new Promise(() => undefined) as never);
        void useCommentStore.getState().retrieveComments("A");
        void useCommentStore.getState().retrieveComments("A");
        expect(get).toHaveBeenCalledTimes(1);
    });

    it("logging out drops a running call, so the next login starts its own", () => {
        start();
        const get = jest
            .spyOn(services.immunization, "getPatientImmunizations")
            .mockReturnValue(new Promise(() => undefined) as never);
        void useImmunizationStore.getState().retrieveImmunizations("A");
        useImmunizationStore.getState().reset();
        void useImmunizationStore.getState().retrieveImmunizations("A");
        expect(get).toHaveBeenCalledTimes(2);
    });

    it("dependents: a normal request joins; a forced one waits, then asks again", async () => {
        start();
        const first = deferred<never[]>();
        const get = jest
            .spyOn(services.dependent, "getAll")
            .mockReturnValueOnce(first.promise)
            .mockResolvedValue([] as never);
        const store = useDependentStore.getState();
        const one = store.retrieveDependents("A", false);
        const joined = store.retrieveDependents("A", false);
        const forced = store.retrieveDependents("A", true);
        expect(get).toHaveBeenCalledTimes(1);
        first.resolve([]);
        await Promise.all([one, joined, forced]);
        expect(get).toHaveBeenCalledTimes(2);
    });

    it("patient data: a call covering these types answers them; other types call again", async () => {
        start();
        const types = [
            PatientDataType.DiagnosticImaging,
            PatientDataType.OrganDonorRegistrationStatus,
        ];
        const call = deferred<{ items: never[] }>();
        const get = jest
            .spyOn(services.patientData, "getPatientData")
            .mockReturnValue(call.promise);
        const store = usePatientDataStore.getState();
        const wide = store.retrievePatientData("A", types);
        const covered = store.retrievePatientData("A", [
            PatientDataType.OrganDonorRegistrationStatus,
        ]);
        expect(get).toHaveBeenCalledTimes(1);
        void store
            .retrievePatientData("A", [PatientDataType.BcCancerScreening])
            .catch(() => undefined);
        expect(get).toHaveBeenCalledTimes(2);
        call.resolve({ items: [] });
        await wide;
        await expect(covered).resolves.toEqual([]);
    });
});
