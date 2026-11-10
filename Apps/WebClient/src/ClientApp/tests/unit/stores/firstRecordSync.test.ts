import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ActionType } from "@/constants/actionType";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { LoadStatus } from "@/models/datasetState";
import { services } from "@/services/container";
import { COPY_DECLINED, recordCache } from "@/services/recordCache";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useErrorStore } from "@/stores/error.store";
import {
    type CachedDataset,
    cachedDatasets,
    clearAllDatasets,
    clearDataset,
    resyncAllDatasets,
    resyncDataset,
    startFirstRecordSync,
} from "@/stores/firstRecordSync";
import { useImmunizationStore } from "@/stores/immunization.store";
import { buildSnapshot, copyExpiresAt } from "@/stores/localSession";
import {
    datasetFailedAt,
    datasetKey,
    datasetRefreshedAt,
    getCopyReadFor,
    resetSavedCopyState,
    retryCopyRead,
} from "@/stores/savedCopyState";
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { ConfigUtil } from "@/utils/configUtil";

const connectivity = () => useConnectivityStore.getState();
const flush = () => new Promise((resolve) => setImmediate(resolve));

const key = (name: string) =>
    datasetKey(useUserStore.getState().user.hdid, name);
const noDependentsOrComments = () =>
    useConfigStore.setState({
        config: {
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                featureToggleConfiguration: {
                    ...configFixture.webClient.featureToggleConfiguration,
                    dependents: { enabled: false },
                    timeline: { comment: false },
                },
            },
        } as never,
    });
const onlyDatasets = (...types: EntryType[]) =>
    jest
        .spyOn(ConfigUtil, "isDatasetEnabled")
        .mockImplementation((t) => types.includes(t));

describe("first record sync", () => {
    let stop: () => void;
    let immunizations: jest.SpiedFunction<
        typeof services.immunization.getPatientImmunizations
    >;

    const start = async () => {
        stop = startFirstRecordSync();
        await flush();
    };

    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        signInRegistered();
        useAppLockStore.setState({ enabled: true, locked: false });
        jest.spyOn(recordCache, "load").mockResolvedValue(undefined);
        jest.spyOn(recordCache, "save").mockResolvedValue(true);
        immunizations = jest
            .spyOn(services.immunization, "getPatientImmunizations")
            .mockRejectedValue(new Error("not now"));
    });

    afterEach(() => {
        stop?.();
        resetSavedCopyState();
        jest.restoreAllMocks();
        useConnectivityStore.setState({
            isOnline: true,
            serviceUnavailable: false,
            syncing: false,
            syncPill: false,
            sessionStatus: "live",
        });
        useAppLockStore.setState({ enabled: false, locked: false });
        resetStores();
    });

    it("runs without a finished copy, quiet until it ends", async () => {
        let finish: (error: Error) => void = () => undefined;
        immunizations.mockReturnValue(
            new Promise((_, reject) => (finish = reject)) as never
        );
        await start();
        expect(immunizations).toHaveBeenCalledTimes(1);
        expect(connectivity().syncing).toBe(true);
        finish(new Error("not now"));
        await flush();
        await flush();
        expect(connectivity().syncing).toBe(false);
    });

    it("only flashes the pill, however long the sync takes", async () => {
        immunizations.mockReturnValue(new Promise(() => undefined) as never);
        await start();
        expect(connectivity().syncPill).toBe(true);
        await new Promise((resolve) => setTimeout(resolve, 1300));
        expect(connectivity().syncPill).toBe(false);
        expect(connectivity().syncing).toBe(true);
    });

    it("does not fetch while a data API is under 24 hours old", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.Immunization);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        jest.spyOn(recordCache, "load").mockResolvedValue({
            version: 2,
            savedAt: 1,
            hdid: "h",
            data: {
                datasets: {},
                refreshed: {
                    [key("immunization")]: Date.now() - 23 * 60 * 60 * 1000,
                },
            },
        });
        await start();
        expect(immunizations).not.toHaveBeenCalled();
        expect(connectivity().syncing).toBe(false);
    });

    it("fetches again once a data API is 24 hours old", async () => {
        jest.spyOn(recordCache, "load").mockResolvedValue({
            version: 2,
            savedAt: Date.now(),
            hdid: "h",
            data: {
                datasets: {},
                refreshed: {
                    [key("immunization")]: Date.now() - 24 * 60 * 60 * 1000,
                },
            },
        });
        await start();
        expect(immunizations).toHaveBeenCalledTimes(1);
    });

    it("opens the saved records on launch, ready without fetching", async () => {
        const hdid = useUserStore.getState().user.hdid;
        noDependentsOrComments();
        onlyDatasets(EntryType.Immunization);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        jest.spyOn(recordCache, "load").mockResolvedValue({
            version: 1,
            savedAt: 1,
            hdid,
            data: {
                refreshed: { [key("immunization")]: Date.now() },
                datasets: {
                    immunization: {
                        data: [{ id: "saved" }],
                        recommendations: [],
                        status: LoadStatus.LOADED,
                        statusMessage: "",
                    },
                },
            },
        });
        await start();
        expect(useImmunizationStore.getState().states[hdid]?.data).toEqual([
            { id: "saved" },
        ]);
        expect(immunizations).not.toHaveBeenCalled();
    });

    it("a real refresh in which everything came back records its time", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.Immunization);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(true);
        immunizations.mockResolvedValue(immunizationResultFixture);
        const patient = jest
            .spyOn(services.patientData, "getPatientData")
            .mockResolvedValue({ items: [] });
        const before = Date.now();
        await start();
        await flush();
        expect(immunizations).toHaveBeenCalledTimes(1);
        expect(patient).toHaveBeenCalledTimes(1);
        expect(datasetRefreshedAt(key("immunization"))).toBeGreaterThanOrEqual(
            before
        );
        expect(datasetRefreshedAt(key("patientData"))).toBeGreaterThanOrEqual(
            before
        );
    });

    it("a fetch that finished without records does not count", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.HealthVisit);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        jest.spyOn(
            services.encounter,
            "getPatientEncounters"
        ).mockResolvedValue({ resultStatus: 0, resourcePayload: [] } as never);
        await start();
        await flush();
        expect(connectivity().syncing).toBe(false);
        expect(datasetRefreshedAt(key("healthVisit"))).toBeUndefined();
        expect(datasetFailedAt(key("healthVisit"))).toBeDefined();
    });

    it("protected medications count: only the user's word can open them", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.Medication);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        jest.spyOn(
            services.medication,
            "getPatientMedicationStatements"
        ).mockResolvedValue({
            resultStatus: ResultType.ActionRequired,
            resultError: { actionCode: ActionType.Protected },
            resourcePayload: [],
        } as never);
        await start();
        await flush();
        expect(datasetRefreshedAt(key("medication"))).toBeDefined();
    });

    it("does not open a saved copy for a login that ended while reading it", async () => {
        const hdid = useUserStore.getState().user.hdid;
        let answer: (copy: never) => void = () => undefined;
        jest.spyOn(recordCache, "load").mockReturnValue(
            new Promise((resolve) => {
                answer = resolve;
            })
        );
        await start();
        useAuthStore.getState().clearStorage();
        answer({
            version: 1,
            savedAt: 1,
            hdid,
            data: {
                refreshed: {},
                datasets: {
                    immunization: {
                        data: [{ id: "saved" }],
                        status: LoadStatus.LOADED,
                    },
                },
            },
        } as never);
        await flush();
        expect(useImmunizationStore.getState().states[hdid]).toBeUndefined();
    });

    it.each([
        ["while the pill shows", true],
        ["after the pill has gone", false],
    ])("logging out mid-sync stops it, %s", async (_, pill) => {
        immunizations.mockReturnValue(new Promise(() => undefined) as never);
        await start();
        expect(connectivity().syncing).toBe(true);
        useConnectivityStore.setState({ syncPill: pill });
        useAuthStore.getState().clearStorage();
        expect(connectivity().syncing).toBe(false);
        expect(connectivity().syncPill).toBe(false);
    });

    it("a refresh in which something failed does not count, and tries again later", async () => {
        await start();
        await flush();
        await flush();
        expect(connectivity().syncing).toBe(false);
        expect(datasetRefreshedAt(key("immunization"))).toBeUndefined();
        expect(immunizations).toHaveBeenCalledTimes(1);
        // nothing changes for five minutes, whatever else happens
        useConnectivityStore.setState({ syncRequests: 9 });
        await flush();
        expect(immunizations).toHaveBeenCalledTimes(1);
        const now = Date.now();
        jest.spyOn(Date, "now").mockReturnValue(now + 5 * 60 * 1000 + 1);
        useConnectivityStore.setState({ syncRequests: 10 });
        await flush();
        expect(immunizations).toHaveBeenCalledTimes(2);
    });

    it("refreshes only the data API that is due", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.Immunization, EntryType.Note);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        immunizations.mockResolvedValue(immunizationResultFixture);
        const notes = jest.spyOn(services.userNote, "getNotes");
        jest.spyOn(recordCache, "load").mockResolvedValue({
            version: 2,
            savedAt: 1,
            hdid: "h",
            data: {
                datasets: {},
                refreshed: { [key("notes")]: Date.now() - 60 * 1000 },
            },
        });
        await start();
        await flush();
        expect(immunizations).toHaveBeenCalledTimes(1);
        expect(notes).not.toHaveBeenCalled();
    });

    it("a failed data API keeps its saved records and doesn't touch the others", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.Immunization, EntryType.Note);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        jest.spyOn(services.userNote, "getNotes").mockResolvedValue({
            resultStatus: ResultType.Success,
            resourcePayload: [],
        } as never);
        const hdid = useUserStore.getState().user.hdid;
        jest.spyOn(recordCache, "load").mockResolvedValue({
            version: 2,
            savedAt: 1,
            hdid,
            data: {
                refreshed: {},
                datasets: {
                    immunization: {
                        data: [{ id: "saved" }],
                        recommendations: [],
                        status: LoadStatus.LOADED,
                        statusMessage: "",
                    },
                },
            },
        });
        await start();
        await flush();
        await flush();
        expect(datasetFailedAt(key("immunization"))).toBeDefined();
        expect(datasetRefreshedAt(key("notes"))).toBeDefined();
        const saved = buildSnapshot(hdid).datasets.immunization as {
            data: { id: string }[];
        };
        expect(saved.data).toEqual([{ id: "saved" }]);
        expect(useImmunizationStore.getState().states[hdid]?.data).toEqual([
            { id: "saved" },
        ]);
    });

    it("a saved failure stays quiet and isn't retried straight away after a restart", async () => {
        noDependentsOrComments();
        onlyDatasets(EntryType.Immunization);
        jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
        jest.spyOn(recordCache, "load").mockResolvedValue({
            version: 2,
            savedAt: 1,
            hdid: "h",
            data: {
                datasets: {},
                failed: { [key("immunization")]: Date.now() - 60 * 1000 },
            },
        });
        await start();
        expect(immunizations).not.toHaveBeenCalled();
        expect(connectivity().syncPill).toBe(false);
    });

    describe("on the Security page", () => {
        const dataset = () =>
            cachedDatasets(useUserStore.getState().user.hdid).find(
                (d) => d.name === "immunization"
            ) as CachedDataset;
        const only = () => {
            noDependentsOrComments();
            onlyDatasets(EntryType.Immunization);
            jest.spyOn(ConfigUtil, "isServiceEnabled").mockReturnValue(false);
            immunizations.mockResolvedValue(immunizationResultFixture);
        };

        it("resync fetches a current data API anyway", async () => {
            only();
            await start();
            await flush();
            expect(immunizations).toHaveBeenCalledTimes(1);
            await resyncDataset(useUserStore.getState().user.hdid, dataset());
            expect(immunizations).toHaveBeenCalledTimes(2);
            expect(connectivity().syncing).toBe(false);
        });

        it("clear removes the records and the time, and it stays cleared", async () => {
            only();
            await start();
            await flush();
            const hdid = useUserStore.getState().user.hdid;
            clearDataset(dataset());
            expect(
                useImmunizationStore.getState().states[hdid]
            ).toBeUndefined();
            expect(datasetRefreshedAt(dataset().key)).toBeUndefined();
            expect(buildSnapshot(hdid).datasets.immunization).toBeUndefined();
            useConnectivityStore.setState({ syncRequests: 3 });
            await flush();
            expect(immunizations).toHaveBeenCalledTimes(1);
            await resyncAllDatasets(hdid);
            expect(immunizations).toHaveBeenCalledTimes(2);
            expect(datasetRefreshedAt(dataset().key)).toBeDefined();
        });

        it("clear all removes only this person's saved copy", async () => {
            only();
            const removeFor = jest
                .spyOn(recordCache, "removeFor")
                .mockResolvedValue();
            const removeEveryone = jest
                .spyOn(recordCache, "remove")
                .mockResolvedValue();
            await start();
            await flush();
            const hdid = useUserStore.getState().user.hdid;
            await clearAllDatasets(hdid);
            expect(removeFor).toHaveBeenCalledWith(hdid);
            expect(removeEveryone).not.toHaveBeenCalled();
            expect(datasetRefreshedAt(dataset().key)).toBeUndefined();
        });
    });

    it("logging out keeps the saved copy; another person's expired one goes at login", async () => {
        const removeExpired = jest
            .spyOn(recordCache, "removeExpired")
            .mockResolvedValue();
        const remove = jest.spyOn(recordCache, "remove").mockResolvedValue();
        await start();
        await flush();
        const hdid = useUserStore.getState().user.hdid;
        expect(removeExpired).toHaveBeenCalledWith(hdid, expect.any(Number));
        const now = Date.now();
        const day = 24 * 60 * 60 * 1000;
        const expiry = (refreshed: Record<string, number>) =>
            copyExpiresAt({ refreshed } as never, now);
        expect(expiry({ a: now - day })).toBe(now);
        expect(expiry({ a: now - day, b: now - 1000 })).toBe(now - 1000 + day);
        expect(expiry({})).toBe(now + day);
        await useAuthStore.getState().signOut();
        expect(remove).not.toHaveBeenCalled();
    });

    describe("when the prompt to read the copy is turned down", () => {
        const decline = () =>
            jest.spyOn(recordCache, "load").mockResolvedValue(COPY_DECLINED);

        it("fetches nothing for the copy, and marks nothing read, so nothing overwrites it", async () => {
            const load = decline();
            await start();
            await flush();
            expect(load).toHaveBeenCalledTimes(1);
            expect(immunizations).not.toHaveBeenCalled();
            expect(getCopyReadFor()).toBeUndefined();
            expect(connectivity().syncing).toBe(false);
        });

        it("doesn't ask again on its own", async () => {
            const load = decline();
            await start();
            useConnectivityStore.setState({ syncRequests: 2 });
            await flush();
            expect(load).toHaveBeenCalledTimes(1);
        });

        it("the first save asks again, and passing carries on as usual", async () => {
            const load = decline();
            await start();
            load.mockResolvedValue(undefined);
            retryCopyRead();
            await flush();
            expect(load).toHaveBeenCalledTimes(2);
            expect(getCopyReadFor()).toBe(useUserStore.getState().user.hdid);
            expect(immunizations).toHaveBeenCalledTimes(1);
        });

        it("turned down twice, it stops for this login", async () => {
            const load = decline();
            await start();
            retryCopyRead();
            await flush();
            expect(load).toHaveBeenCalledTimes(2);
            retryCopyRead();
            useConnectivityStore.setState({ syncRequests: 2 });
            await flush();
            expect(load).toHaveBeenCalledTimes(2);
            expect(immunizations).not.toHaveBeenCalled();
        });

        it("the next login asks afresh", async () => {
            const load = decline();
            await start();
            retryCopyRead();
            await flush();
            useAuthStore.getState().clearStorage();
            load.mockClear();
            signInRegistered();
            await flush();
            expect(load).toHaveBeenCalledTimes(1);
        });
    });

    it("needs device security: nothing would be saved without it", async () => {
        useAppLockStore.setState({ enabled: false });
        await start();
        expect(immunizations).not.toHaveBeenCalled();
    });

    it("waits for a working connection, then runs", async () => {
        useConnectivityStore.setState({ isOnline: false });
        await start();
        expect(immunizations).not.toHaveBeenCalled();
        useConnectivityStore.setState({ isOnline: true });
        await flush();
        expect(immunizations).toHaveBeenCalledTimes(1);
    });

    it("runs once per login", async () => {
        await start();
        await flush();
        useConnectivityStore.setState({ syncRequests: 5 });
        await flush();
        expect(immunizations).toHaveBeenCalledTimes(1);
    });

    it("keeps the first sync's retrieval failures off the page", () => {
        useConnectivityStore.setState({ syncing: true });
        const errors = useErrorStore.getState();
        errors.addError(ErrorType.Retrieve, ErrorSourceType.User);
        errors.setTooManyRequestsWarning("page");
        expect(useErrorStore.getState().errors).toHaveLength(0);
        expect(useErrorStore.getState().tooManyRequestsWarning).toBeUndefined();
    });

    it("still shows a failure the user caused", () => {
        useConnectivityStore.setState({ syncing: true });
        useErrorStore
            .getState()
            .addError(ErrorType.Update, ErrorSourceType.User);
        expect(useErrorStore.getState().errors).toHaveLength(1);
    });

    it("the saved copy holds dependents and what loaded for them", () => {
        const dependent = { ownerId: "D1" };
        useDependentStore.setState({
            dependents: [dependent] as never,
            status: LoadStatus.LOADED,
        });
        useImmunizationStore.setState({
            states: {
                D1: { status: LoadStatus.LOADED, data: [{ id: "i" }] },
            } as never,
        });
        const snapshot = buildSnapshot("SELF");
        expect(snapshot.dependents).toEqual([dependent]);
        expect(snapshot.dependentDatasets?.D1).toHaveProperty("immunization");
    });
});

describe("signed out", () => {
    it("does nothing", async () => {
        useAuthStore.setState({ isAuthenticated: false });
        const load = jest.spyOn(recordCache, "load");
        const stop = startFirstRecordSync();
        await flush();
        expect(load).not.toHaveBeenCalled();
        stop();
        jest.restoreAllMocks();
    });
});
