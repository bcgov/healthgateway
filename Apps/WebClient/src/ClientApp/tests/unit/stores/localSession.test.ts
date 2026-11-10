import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { LoadStatus } from "@/models/datasetState";
import { services } from "@/services/container";
import { recordCache } from "@/services/recordCache";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useErrorStore } from "@/stores/error.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import {
    beginRelogin,
    buildSnapshot,
    prepareLocalSession,
    probeService,
    recoverSession,
    restoreLocalSession,
    startRecordCacheSync,
} from "@/stores/localSession";
import { useMedicationStore } from "@/stores/medication.store";
import { useNoteStore } from "@/stores/note.store";
import {
    datasetKey,
    datasetRefreshedAt,
    markDatasetRefreshed,
    setCopyReadFor,
    setCopyReadRetry,
    setLastCopy,
} from "@/stores/savedCopyState";
import { useUserStore } from "@/stores/user.store";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(async () => undefined),
    deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock("@/services/vault", () => ({
    vault: {
        unlock: jest.fn(),
        lock: jest.fn(),
        destroy: jest.fn(async () => undefined),
    },
}));
jest.mock("@/services/recordCache", () => ({
    recordCache: {
        save: jest.fn(async () => true),
        load: jest.fn(async () => undefined),
        remove: jest.fn(async () => undefined),
    },
}));

const HDID = "HDID1";
const details = (expired: boolean) =>
    ({ idToken: "id", accessToken: "a", expired, hdid: HDID }) as never;

const useAuth = (expired: boolean, failure?: "rejected" | "network") => {
    const auth = fakeAuth(details(expired));
    (auth as { getRestoreFailure?: () => unknown }).getRestoreFailure = () =>
        failure;
    services.initialize(configFixture, auth);
    return auth;
};

const seedImmunizations = () =>
    useImmunizationStore.setState({
        states: {
            [HDID]: {
                data: [{ id: "i1" }],
                status: LoadStatus.LOADED,
                statusMessage: "",
                recommendations: [],
            },
        },
    } as never);

describe("local session", () => {
    const connectivity = () => useConnectivityStore.getState();

    beforeEach(() => {
        jest.clearAllMocks();
        jest.mocked(recordCache.load).mockResolvedValue(undefined);
        useAppLockStore.setState({
            enabled: true,
            locked: false,
            beforeUnlock: undefined,
        });
        useConnectivityStore.setState({
            isOnline: true,
            serviceUnavailable: false,
            sessionStatus: "live",
            savedAt: undefined,
            syncRequests: 0,
        });
    });

    afterEach(() => {
        jest.useRealTimers();
        resetStores();
    });

    describe("prepareLocalSession", () => {
        it("does nothing when the login works", () => {
            useAuth(false);
            expect(prepareLocalSession()).toBe(false);
            expect(useAppLockStore.getState().locked).toBe(false);
        });

        it("offline with device security: locks, to open the saved copy after the prompt", async () => {
            useAuth(false);
            useConnectivityStore.setState({ isOnline: false });
            expect(prepareLocalSession()).toBe(true);
            const lock = useAppLockStore.getState();
            expect(lock.locked).toBe(true);
            expect(lock.beforeUnlock).toBeDefined();

            await lock.beforeUnlock?.();
            expect(recordCache.load).toHaveBeenCalledWith(HDID);
        });

        it("a refresh that could not reach Keycloak counts as offline, not as an expired login", async () => {
            useAuth(true, "network");
            expect(prepareLocalSession()).toBe(true);
            expect(connectivity().serviceUnavailable).toBe(true);
            jest.mocked(recordCache.load).mockResolvedValue({
                version: 1,
                savedAt: 5,
                hdid: HDID,
                data: { user: { hdid: HDID }, datasets: {} },
            });
            await useAppLockStore.getState().beforeUnlock?.();
            expect(connectivity().sessionStatus).toBe("offline");
        });

        it("a refused refresh token, online, is the expired session", async () => {
            useAuth(true, "rejected");
            jest.mocked(recordCache.load).mockResolvedValue({
                version: 1,
                savedAt: 5,
                hdid: HDID,
                data: { user: { hdid: HDID }, datasets: {} },
            });
            expect(prepareLocalSession()).toBe(true);
            await useAppLockStore.getState().beforeUnlock?.();
            expect(connectivity().sessionStatus).toBe("expired");
            expect(connectivity().savedAt).toBe(5);
        });

        it("no device security: nothing opens offline", () => {
            useAuth(false);
            useAppLockStore.setState({ enabled: false });
            useConnectivityStore.setState({ isOnline: false });
            expect(prepareLocalSession()).toBe(false);
            expect(useAppLockStore.getState().locked).toBe(false);
        });

        it("no session left: keeps the copy for that person's next login", () => {
            services.initialize(configFixture, fakeAuth());
            expect(prepareLocalSession()).toBe(false);
            expect(recordCache.remove).not.toHaveBeenCalled();
        });
    });

    describe("restoreLocalSession", () => {
        it("opens the saved copy and lets the user in", async () => {
            services.initialize(configFixture, fakeAuth());
            jest.mocked(recordCache.load).mockResolvedValue({
                version: 1,
                savedAt: 7,
                hdid: HDID,
                data: {
                    user: { hdid: HDID, acceptedTermsOfService: true },
                    datasets: {
                        immunization: {
                            data: [{ id: "i1" }],
                            status: LoadStatus.LOADED,
                        },
                    },
                },
            });
            await restoreLocalSession(HDID, "offline");
            expect(useAuthStore.getState().isAuthenticated).toBe(true);
            expect(useUserStore.getState().user.acceptedTermsOfService).toBe(
                true
            );
            expect(useUserStore.getState().essentialDataLoaded).toBe(true);
            expect(useImmunizationStore.getState().states[HDID].status).toBe(
                LoadStatus.LOADED
            );
            expect(connectivity().sessionStatus).toBe("offline");
            expect(connectivity().savedAt).toBe(7);
        });

        it("with no saved copy, falls back to a normal login", async () => {
            services.initialize(configFixture, fakeAuth());
            await restoreLocalSession(HDID, "offline");
            expect(useAuthStore.getState().isAuthenticated).toBe(false);
        });
    });

    describe("buildSnapshot", () => {
        it("holds only what finished loading", () => {
            seedImmunizations();
            useNoteStore.setState({ status: LoadStatus.REQUESTED });
            const snapshot = buildSnapshot(HDID);
            expect(snapshot.datasets.immunization).toBeDefined();
            expect(snapshot.notes).toBeUndefined();
        });

        it("includes medications opened with a protective word", () => {
            useMedicationStore.setState({
                states: {
                    [HDID]: {
                        data: [{ id: "m1" }],
                        status: LoadStatus.LOADED,
                        statusMessage: "",
                        protectiveWordAttempts: 1,
                    },
                },
            } as never);
            expect(buildSnapshot(HDID).datasets.medication).toBeDefined();
        });

        it("leaves out medications still waiting for the word", () => {
            useMedicationStore.setState({
                states: {
                    [HDID]: {
                        data: [],
                        status: LoadStatus.PROTECTED,
                        statusMessage: "",
                        protectiveWordAttempts: 1,
                    },
                },
            } as never);
            expect(buildSnapshot(HDID).datasets.medication).toBeUndefined();
        });
    });

    describe("logging in again after a session expired", () => {
        const seed = () => {
            useUserStore.setState({
                user: { ...useUserStore.getState().user, hdid: HDID },
            });
            useNoteStore.setState({
                notes: [{ id: "n1", title: "first person's" }] as never,
                status: LoadStatus.LOADED,
            });
            useImmunizationStore.setState({
                states: {
                    [HDID]: {
                        data: [{ id: "saved" }],
                        recommendations: [],
                        status: LoadStatus.LOADED,
                        statusMessage: "",
                    },
                },
            } as never);
            markDatasetRefreshed(datasetKey(HDID, "notes"), Date.now());
            beginRelogin();
        };
        const signInAs = async (hdid: string) => {
            services.initialize(configFixture, {
                ...fakeAuth(),
                signIn: jest.fn(async () => ({
                    idToken: "i",
                    accessToken: "a",
                    refreshTokenTime: Date.now() / 1000 + 600,
                })),
                getOidcUserInfo: jest.fn(async () => ({ hdid })),
            } as never);
            await useAuthStore.getState().signIn("/home");
        };

        it("a different person sees nothing of the first person's", async () => {
            seed();
            await signInAs("SOMEONE-ELSE");
            expect(useNoteStore.getState().notes).toEqual([]);
            expect(useNoteStore.getState().status).toBe(LoadStatus.NONE);
            expect(
                useImmunizationStore.getState().states[HDID]
            ).toBeUndefined();
            expect(
                datasetRefreshedAt(datasetKey(HDID, "notes"))
            ).toBeUndefined();
            expect(buildSnapshot("SOMEONE-ELSE").notes).toBeUndefined();
        });

        it("the same person keeps their records", async () => {
            seed();
            await signInAs(HDID);
            expect(useNoteStore.getState().notes).toHaveLength(1);
            expect(
                useImmunizationStore.getState().states[HDID]?.data
            ).toHaveLength(1);
            expect(datasetRefreshedAt(datasetKey(HDID, "notes"))).toBeDefined();
        });
    });

    describe("beginRelogin", () => {
        it("drops the refused session and leaves the banner state", () => {
            const auth = useAuth(true, "rejected");
            useConnectivityStore.setState({ sessionStatus: "expired" });
            useAuthStore.setState({ isAuthenticated: true });
            beginRelogin();
            expect(auth.clearState).toHaveBeenCalled();
            expect(useAuthStore.getState().isAuthenticated).toBe(false);
            expect(connectivity().sessionStatus).toBe("live");
        });
    });

    describe("recoverSession", () => {
        it("signing out while it waits leaves nothing behind", async () => {
            useAuth(false);
            let refreshed: (value: boolean) => void = () => undefined;
            const auth = services.auth;
            jest.spyOn(auth, "refreshToken").mockReturnValue(
                new Promise<boolean>((resolve) => {
                    refreshed = resolve;
                })
            );
            const info = jest.spyOn(auth, "getOidcUserInfo");
            const recovering = recoverSession();
            await useAuthStore.getState().signOut();
            refreshed(true);
            await recovering;
            expect(info).not.toHaveBeenCalled();
            expect(useUserStore.getState().user.hdid).toBe("");
        });

        it("signing out during its user info writes no identity", async () => {
            useAuth(false);
            let answer: (value: never) => void = () => undefined;
            const auth = services.auth;
            jest.spyOn(auth, "refreshToken").mockResolvedValue(true);
            jest.spyOn(auth, "getOidcUserInfo").mockReturnValue(
                new Promise((resolve) => {
                    answer = resolve;
                })
            );
            const recovering = recoverSession();
            await new Promise((resolve) => setImmediate(resolve));
            await useAuthStore.getState().signOut();
            answer({ hdid: "USER-A" } as never);
            await recovering;
            expect(useUserStore.getState().user.hdid).toBe("");
            expect(useErrorStore.getState().errors).toHaveLength(0);
        });

        it("device security: a live session keeps the saved copy; its age decides", async () => {
            useAuth(false);
            useAppLockStore.setState({ enabled: true });
            useUserStore.setState({
                user: Object.assign(useUserStore.getState().user, {
                    hdid: HDID,
                }),
            });
            seedImmunizations();
            await recoverSession();
            expect(useImmunizationStore.getState().states[HDID].status).toBe(
                LoadStatus.LOADED
            );
            expect(connectivity().syncRequests).toBe(0);
        });

        it("no device security: a live session just syncs again", async () => {
            useAuth(false);
            useAppLockStore.setState({ enabled: false });
            useUserStore.setState({
                user: Object.assign(useUserStore.getState().user, {
                    hdid: HDID,
                }),
            });
            seedImmunizations();
            await recoverSession();
            expect(useImmunizationStore.getState().states[HDID].status).toBe(
                LoadStatus.NONE
            );
            expect(connectivity().syncRequests).toBe(1);
        });

        it("offline session: a refused token becomes the expired banner", async () => {
            const auth = useAuth(true);
            jest.mocked(auth.refreshToken).mockRejectedValue({
                code: "invalid_grant",
            });
            useConnectivityStore.setState({
                sessionStatus: "offline",
                savedAt: 9,
            });
            await recoverSession();
            expect(connectivity().sessionStatus).toBe("expired");
            expect(connectivity().savedAt).toBe(9);
        });

        it("offline session: another network failure stays offline", async () => {
            const auth = useAuth(true);
            jest.mocked(auth.refreshToken).mockRejectedValue(
                new TypeError("Network request failed")
            );
            useConnectivityStore.setState({ sessionStatus: "offline" });
            await recoverSession();
            expect(connectivity().sessionStatus).toBe("offline");
            expect(connectivity().syncRequests).toBe(0);
        });
    });

    it("opening the saved copy brings back dependents and their records", async () => {
        useAuth(false);
        useConnectivityStore.setState({ isOnline: false });
        jest.mocked(recordCache.load).mockResolvedValue({
            version: 1,
            savedAt: 5,
            hdid: HDID,
            data: {
                user: { hdid: HDID },
                datasets: {},
                dependents: [{ ownerId: "D1" }],
                dependentDatasets: {
                    D1: {
                        immunization: {
                            data: [{ id: "i1" }],
                            status: LoadStatus.LOADED,
                        },
                    },
                },
            },
        });
        await restoreLocalSession(HDID, "offline");
        expect(useDependentStore.getState().dependents).toEqual([
            { ownerId: "D1" },
        ]);
        expect(useDependentStore.getState().status).toBe(LoadStatus.LOADED);
        expect(useImmunizationStore.getState().states.D1?.status).toBe(
            LoadStatus.LOADED
        );
    });

    describe("a refresh that does not finish", () => {
        const person = (status: LoadStatus, data: unknown[]) =>
            useImmunizationStore.setState({
                states: {
                    [HDID]: {
                        data,
                        recommendations: [],
                        status,
                        statusMessage: "",
                    },
                },
            } as never);
        const saved = () =>
            buildSnapshot(HDID).datasets.immunization as
                { status: LoadStatus; data: unknown[] } | undefined;

        it("the copy keeps the last good records of a failed or unfinished refresh", () => {
            person(LoadStatus.ERROR, [{ id: "old" }]);
            expect(saved()).toEqual(
                expect.objectContaining({
                    status: LoadStatus.LOADED,
                    data: [{ id: "old" }],
                })
            );
            person(LoadStatus.NONE, [{ id: "old" }]);
            expect(saved()?.data).toEqual([{ id: "old" }]);
        });

        it("a first load that failed adds nothing to the copy", () => {
            person(LoadStatus.ERROR, []);
            expect(saved()).toBeUndefined();
        });

        describe("saving", () => {
            let stop: () => void;
            const settle = async () => {
                await jest.advanceTimersByTimeAsync(2100);
            };
            beforeEach(() => {
                jest.useFakeTimers();
                useAppLockStore.setState({ enabled: true, locked: false });
                useAuthStore.setState({ isAuthenticated: true });
                useUserStore.setState({
                    essentialDataLoaded: true,
                    user: Object.assign(useUserStore.getState().user, {
                        hdid: HDID,
                    }),
                });
                jest.mocked(recordCache.save).mockClear();
                setCopyReadFor(HDID);
                stop = startRecordCacheSync();
            });
            afterEach(() => {
                stop();
                useConnectivityStore.setState({ pullInProgress: false });
                useAppLockStore.setState({ enabled: false });
            });

            it("waits for a pull to finish, then saves", async () => {
                useConnectivityStore.setState({ pullInProgress: true });
                person(LoadStatus.LOADED, [{ id: "new" }]);
                await settle();
                expect(recordCache.save).not.toHaveBeenCalled();
                useConnectivityStore.setState({ pullInProgress: false });
                await settle();
                expect(recordCache.save).toHaveBeenCalledTimes(1);
            });

            it("still saves while something is being fetched, keeping that one's last good records", async () => {
                setLastCopy({
                    datasets: {
                        immunization: { data: [{ id: "saved" }] },
                    },
                });
                person(LoadStatus.REQUESTED, []);
                jest.advanceTimersByTime(2000);
                expect(recordCache.save).toHaveBeenCalledTimes(1);
                expect(saved()?.data).toEqual([{ id: "saved" }]);
            });

            it("doesn't save before the copy is read, which would overwrite it", () => {
                setCopyReadFor(undefined);
                person(LoadStatus.LOADED, [{ id: "new" }]);
                jest.advanceTimersByTime(2000);
                expect(recordCache.save).not.toHaveBeenCalled();
            });
        });
    });

    describe("probeService", () => {
        afterEach(() => {
            jest.restoreAllMocks();
        });

        it("any answer clears unavailable, even a refusal", async () => {
            useConnectivityStore.setState({ serviceUnavailable: true });
            jest.spyOn(global, "fetch").mockResolvedValue(
                new Response(null, { status: 401 })
            );
            await probeService();
            expect(connectivity().serviceUnavailable).toBe(false);
        });

        it("no answer leaves it unavailable, quietly", async () => {
            useConnectivityStore.setState({ serviceUnavailable: true });
            jest.spyOn(global, "fetch").mockRejectedValue(new Error("down"));
            await expect(probeService()).resolves.toBeUndefined();
            expect(connectivity().serviceUnavailable).toBe(true);
        });
    });

    describe("startRecordCacheSync", () => {
        const signedIn = () => {
            useAuthStore.setState({ isAuthenticated: true });
            useUserStore.setState({
                essentialDataLoaded: true,
                user: Object.assign(useUserStore.getState().user, {
                    hdid: HDID,
                }),
            });
        };

        it("saves shortly after records change", () => {
            jest.useFakeTimers();
            signedIn();
            setCopyReadFor(HDID);
            const stop = startRecordCacheSync();
            seedImmunizations();
            jest.advanceTimersByTime(1999);
            expect(recordCache.save).not.toHaveBeenCalled();
            jest.advanceTimersByTime(1);
            expect(recordCache.save).toHaveBeenCalledTimes(1);
            stop();
        });

        describe("before the copy has been read", () => {
            it("saves nothing, and asks to read it again once there are records", () => {
                jest.useFakeTimers();
                signedIn();
                setCopyReadFor(undefined);
                setLastCopy(undefined);
                useImmunizationStore.setState({ states: {} });
                const retry = jest.fn();
                setCopyReadRetry(retry);
                const stop = startRecordCacheSync();
                useConnectivityStore.setState({ syncRequests: 7 });
                jest.advanceTimersByTime(3000);
                expect(retry).not.toHaveBeenCalled();
                seedImmunizations();
                jest.advanceTimersByTime(3000);
                expect(retry).toHaveBeenCalledTimes(1);
                expect(recordCache.save).not.toHaveBeenCalled();
                setCopyReadRetry(undefined);
                stop();
            });
        });

        it("records when the copy expires", () => {
            jest.useFakeTimers();
            signedIn();
            setCopyReadFor(HDID);
            const stop = startRecordCacheSync();
            seedImmunizations();
            jest.advanceTimersByTime(2000);
            const [, , savedAt, expiresAt] = jest.mocked(recordCache.save).mock
                .calls[0];
            expect(expiresAt).toBe(savedAt + 24 * 60 * 60 * 1000);
            stop();
        });

        it.each([
            [
                "device security is off",
                () => useAppLockStore.setState({ enabled: false }),
            ],
            [
                "the app is locked",
                () => useAppLockStore.setState({ locked: true }),
            ],
            [
                "offline",
                () => useConnectivityStore.setState({ isOnline: false }),
            ],
            [
                "the login expired",
                () =>
                    useConnectivityStore.setState({ sessionStatus: "expired" }),
            ],
        ])("does not save when %s", (_name, arrange) => {
            jest.useFakeTimers();
            signedIn();
            arrange();
            const stop = startRecordCacheSync();
            seedImmunizations();
            jest.advanceTimersByTime(3000);
            expect(recordCache.save).not.toHaveBeenCalled();
            stop();
        });
    });
});
