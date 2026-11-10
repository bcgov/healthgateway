import { LoadStatus } from "@/models/datasetState";
import { isSessionEnded } from "@/models/errors";
import type { Patient } from "@/models/patient";
import { type OidcUserInfo, User } from "@/models/user";
import { classifyAuthFailure } from "@/services/auth/authFailure";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { COPY_DECLINED, recordCache } from "@/services/recordCache";
import { sessionGeneration } from "@/services/sessionFence";

import { useAppLockStore } from "./appLock.store";
import { useAuthStore } from "./auth.store";
import { useClinicalDocumentStore } from "./clinicalDocument.store";
import { useCommentStore } from "./comment.store";
import { getConfigUrl, useConfigStore } from "./config.store";
import {
    selectCanFetch,
    type SessionStatus,
    useConnectivityStore,
} from "./connectivity.store";
import { useCovid19TestResultStore } from "./covid19TestResult.store";
import { useDependentStore } from "./dependent.store";
import { useHealthVisitStore } from "./healthVisit.store";
import { useHospitalVisitStore } from "./hospitalVisit.store";
import { useImmunizationStore } from "./immunization.store";
import { useLabResultStore } from "./labResult.store";
import { useMedicationStore } from "./medication.store";
import { useNoteStore } from "./note.store";
import { usePatientDataStore } from "./patientData.store";
import { setRecordOwner } from "./recordOwner";
import { markTimelineDatasetsStale } from "./refreshDatasets";
import {
    getCopyReadFor,
    getFailedTimes,
    getLastCopy,
    getRefreshedTimes,
    REFRESH_AFTER_MS,
    retryCopyRead,
    setLastCopy,
    setSavedTimes,
} from "./savedCopyState";
import { useSpecialAuthorityRequestStore } from "./specialAuthorityRequest.store";
import { useUserStore } from "./user.store";

/** Per-person dataset stores, by the name their state has in the snapshot. */
const perPersonStores = {
    immunization: useImmunizationStore,
    labResult: useLabResultStore,
    covid19TestResult: useCovid19TestResultStore,
    healthVisit: useHealthVisitStore,
    specialAuthorityRequest: useSpecialAuthorityRequestStore,
    clinicalDocument: useClinicalDocumentStore,
    hospitalVisit: useHospitalVisitStore,
    patientData: usePatientDataStore,
    medication: useMedicationStore,
} as const;
type PerPersonStore = {
    getState: () => { states: Record<string, unknown> };
    setState: (partial: { states: Record<string, unknown> }) => void;
};
const stores = perPersonStores as unknown as Record<string, PerPersonStore>;

export interface SnapshotData {
    user: User;
    patient?: Patient;
    oidcUserInfo?: OidcUserInfo;
    datasets: Record<string, unknown>;
    notes?: ReturnType<typeof useNoteStore.getState>["notes"];
    comments?: ReturnType<typeof useCommentStore.getState>["comments"];
    dependents?: ReturnType<typeof useDependentStore.getState>["dependents"];
    /** by dependent's hdid: what finished loading */
    dependentDatasets?: Record<string, Record<string, unknown>>;
    /** when each data API last came back complete, by `datasetKey`; one with none has not */
    refreshed?: Record<string, number>;
    /** when each data API last failed to, by `datasetKey`, so restarting the app doesn't retry it straight away */
    failed?: Record<string, number>;
}

const isLoaded = (state: unknown) =>
    (state as { status?: LoadStatus } | undefined)?.status ===
    LoadStatus.LOADED;

/**
 * What the copy can hold: records that finished loading, or the last good ones of a refresh that failed or was never
 * finished, so a later save never loses them. Medications opened with a protective word are included: the word guards the fetch.
 */
const hasRecords = (state: unknown) => {
    const { status, data } = (state ?? {}) as {
        status?: LoadStatus;
        data?: unknown[] | Record<string, unknown>;
    };
    if (status === LoadStatus.LOADED) return true;
    if (status !== LoadStatus.NONE && status !== LoadStatus.ERROR) return false;
    return data !== undefined && Object.keys(data).length > 0;
};

export function buildSnapshot(hdid: string): SnapshotData {
    const { user, patient, oidcUserInfo } = useUserStore.getState();
    const lastCopy = getLastCopy() as SnapshotData | undefined;
    const loadedFor = (person: string) => {
        const previous =
            person === hdid
                ? lastCopy?.datasets
                : lastCopy?.dependentDatasets?.[person];
        const datasets: Record<string, unknown> = {};
        for (const [name, store] of Object.entries(stores)) {
            const state = store.getState().states[person];
            if (hasRecords(state))
                datasets[name] = {
                    ...(state as object),
                    status: LoadStatus.LOADED,
                };
            else if (previous?.[name]) datasets[name] = previous[name];
        }
        return datasets;
    };
    const dependentState = useDependentStore.getState();
    const dependents =
        dependentState.status === LoadStatus.LOADED
            ? dependentState.dependents
            : lastCopy?.dependents;
    const notes = useNoteStore.getState();
    const comments = useCommentStore.getState();
    return {
        user,
        patient,
        oidcUserInfo,
        datasets: loadedFor(hdid),
        dependents,
        dependentDatasets: dependents
            ? Object.fromEntries(
                  dependents.map((d) => [d.ownerId, loadedFor(d.ownerId)])
              )
            : undefined,
        refreshed: getRefreshedTimes(),
        failed: getFailedTimes(),
        notes:
            notes.status === LoadStatus.LOADED ? notes.notes : lastCopy?.notes,
        comments:
            comments.status === LoadStatus.LOADED
                ? comments.comments
                : lastCopy?.comments,
    };
}

/** A clear by hand: a later save must not bring these back from the last copy. */
export function forgetFromLastCopy(person: string, name: string) {
    const copy = getLastCopy() as SnapshotData | undefined;
    if (!copy) return;
    const owner = useUserStore.getState().user.hdid;
    const drop = (datasets?: Record<string, unknown>) =>
        datasets &&
        Object.fromEntries(
            Object.entries(datasets).filter(([key]) => key !== name)
        );
    setLastCopy({
        ...copy,
        datasets: person === owner ? drop(copy.datasets) : copy.datasets,
        dependentDatasets:
            person === owner
                ? copy.dependentDatasets
                : {
                      ...copy.dependentDatasets,
                      [person]: drop(copy.dependentDatasets?.[person]) ?? {},
                  },
        notes: name === "notes" ? undefined : copy.notes,
        comments: name === "comments" ? undefined : copy.comments,
        dependents: name === "dependents" ? undefined : copy.dependents,
    });
}

/** Puts the saved records in the stores, so the pages have them without fetching. What already loaded stays. */
export function seedRecords(hdid: string, data: SnapshotData) {
    const seedPerson = (person: string, datasets: Record<string, unknown>) => {
        for (const [name, state] of Object.entries(datasets)) {
            const store = stores[name];
            if (store && !isLoaded(store.getState().states[person]))
                store.setState({
                    states: { ...store.getState().states, [person]: state },
                });
        }
    };
    seedPerson(hdid, data.datasets);
    for (const [person, datasets] of Object.entries(
        data.dependentDatasets ?? {}
    ))
        seedPerson(person, datasets);
    if (
        data.dependents &&
        useDependentStore.getState().status !== LoadStatus.LOADED
    )
        useDependentStore.setState({
            dependents: data.dependents,
            status: LoadStatus.LOADED,
        });
    setSavedTimes(data.refreshed ?? {}, data.failed ?? {});
    setLastCopy(data);
    if (data.notes && useNoteStore.getState().status !== LoadStatus.LOADED)
        useNoteStore.setState({
            notes: data.notes,
            status: LoadStatus.LOADED,
        });
    if (
        data.comments &&
        useCommentStore.getState().status !== LoadStatus.LOADED
    )
        useCommentStore.setState({
            comments: data.comments,
            status: LoadStatus.LOADED,
        });
}

function seedStores(hdid: string, data: SnapshotData) {
    seedRecords(hdid, data);
    useUserStore.setState({
        user: Object.assign(new User(), data.user),
        patient: data.patient,
        oidcUserInfo: data.oidcUserInfo,
        essentialDataLoaded: true,
    });
}

/**
 * Passing the lock's prompt on a launch with no working login: opens the saved copy. With no copy to open there is
 * nothing to show, so the user is signed out to the normal login.
 */
export async function restoreLocalSession(
    hdid: string,
    status: Exclude<SessionStatus, "live">
): Promise<void> {
    const snapshot = await recordCache.load<SnapshotData>(hdid);
    // turned down: the lock's prompt has only just passed, so this is rare, and there is nothing else to show
    if (!snapshot || snapshot === COPY_DECLINED) {
        logger.debug("No saved records to open");
        useAuthStore.getState().clearStorage();
        return;
    }
    seedStores(hdid, snapshot.data);
    setRecordOwner(hdid);
    // the oldest refresh is the fairest answer to how old the records are
    const refreshed = Object.values(snapshot.data.refreshed ?? {});
    useConnectivityStore
        .getState()
        .setSessionStatus(
            status,
            refreshed.length > 0 ? Math.min(...refreshed) : snapshot.savedAt
        );
    useAuthStore.setState({ isAuthenticated: true });
}

/**
 * Launch with device security and a saved session that cannot work right now: the app locks, and opens the saved copy
 * once unlocked. Returns whether that is what is happening.
 */
export function prepareLocalSession(): boolean {
    const lock = useAppLockStore.getState();
    if (!lock.enabled || !services.isInitialized) return false;
    const details = services.auth.getOidcTokenDetails();
    if (!details) {
        // no session to open it for: it stays for that person's next login
        return false;
    }
    const failure = services.auth.getRestoreFailure?.();
    const { isOnline } = useConnectivityStore.getState();
    // a refresh that got no answer says nothing about the login, however old the access token is
    const status: Exclude<SessionStatus, "live"> | undefined =
        failure === "rejected"
            ? "expired"
            : failure === "network" || !isOnline
              ? "offline"
              : details.expired
                ? "expired"
                : undefined;
    if (!status) return false;
    if (status === "offline" && isOnline)
        useConnectivityStore.getState().setServiceUnavailable(true);
    useAppLockStore.setState({
        locked: true,
        beforeUnlock: () => restoreLocalSession(details.hdid, status),
    });
    return true;
}

/** The expired banner's link: drops the refused session, so the login that follows starts fresh. */
export function beginRelogin(): void {
    // the records stay on screen behind the login, which may turn out to be someone else's (auth.store signIn)
    setRecordOwner(useUserStore.getState().user.hdid || undefined);
    services.http.endSession();
    services.auth.clearState();
    services.http.unsetAuthorizationHeader();
    useUserStore.getState().clearUserData();
    useConnectivityStore.getState().setSessionStatus("live");
    useAuthStore.setState({ tokenDetails: undefined, isAuthenticated: false });
}

/** Back online, or Health Gateway is back: a login that was on hold works again, or turns out to have expired. */
export async function recoverSession(): Promise<void> {
    const session = sessionGeneration();
    const ended = () => session !== sessionGeneration();
    const connectivity = useConnectivityStore.getState();
    const hdid = useUserStore.getState().user.hdid;
    if (connectivity.sessionStatus === "expired") return;
    if (connectivity.sessionStatus === "offline") {
        try {
            const refreshed = await services.auth.refreshToken(true);
            if (ended()) return;
            const details = services.auth.getOidcTokenDetails();
            if (!refreshed || !details || details.expired) {
                connectivity.setSessionStatus("expired", connectivity.savedAt);
                return;
            }
            useAuthStore.getState().handleSuccessfulAuthentication(details);
            const info = await services.auth.getOidcUserInfo();
            if (ended()) return;
            useUserStore.getState().setOidcUserInfo(info);
            await useUserStore.getState().retrieveEssentialData();
            if (ended()) return;
        } catch (error) {
            if (ended() || isSessionEnded(error)) return;
            if (classifyAuthFailure(error) === "connection") {
                logger.debug("Still cannot reach login");
                useConnectivityStore.getState().setServiceUnavailable(true);
            } else
                connectivity.setSessionStatus("expired", connectivity.savedAt);
            return;
        }
    }
    // Device security: the saved copy stays until it is old enough (firstRecordSync) or refreshed by hand
    if (useAppLockStore.getState().enabled) return;
    if (hdid) markTimelineDatasetsStale(hdid);
    useConnectivityStore.getState().requestSync();
}

/** Nothing in a copy is still good once its newest data API has expired. */
export const copyExpiresAt = (copy: SnapshotData, savedAt: number) => {
    const times = Object.values(copy.refreshed ?? {});
    return (times.length > 0 ? Math.max(...times) : savedAt) + REFRESH_AFTER_MS;
};

/** How often the saved copy may be rewritten while records keep arriving. */
const SAVE_DEBOUNCE_MS = 2000;

/** Keeps the saved copy current. Native; only while unlocked with device security on and a working login. */
export function startRecordCacheSync(): () => void {
    let timer: ReturnType<typeof setTimeout> | undefined;
    const save = () => {
        timer = undefined;
        const lock = useAppLockStore.getState();
        const { essentialDataLoaded, user } = useUserStore.getState();
        if (
            !lock.enabled ||
            lock.locked ||
            useConnectivityStore.getState().pullInProgress ||
            !essentialDataLoaded ||
            !user.hdid ||
            !selectCanFetch(useConnectivityStore.getState()) ||
            !useAuthStore.getState().isAuthenticated
        )
            return;
        const copy = buildSnapshot(user.hdid);
        // saved before the copy is read, it would overwrite it with next to nothing
        if (getCopyReadFor() !== user.hdid) {
            if (Object.keys(copy.datasets).length > 0) retryCopyRead();
            return;
        }
        setLastCopy(copy);
        const now = Date.now();
        void recordCache.save(user.hdid, copy, now, copyExpiresAt(copy, now));
    };
    const schedule = () => {
        if (timer !== undefined) clearTimeout(timer);
        timer = setTimeout(save, SAVE_DEBOUNCE_MS);
    };
    const unsubscribes = [
        ...Object.values(perPersonStores).map((store) =>
            (
                store as unknown as { subscribe: (l: () => void) => () => void }
            ).subscribe(schedule)
        ),
        useNoteStore.subscribe(schedule),
        useCommentStore.subscribe(schedule),
        useDependentStore.subscribe(schedule),
        useConnectivityStore.subscribe(schedule),
        useUserStore.subscribe(schedule),
        useAppLockStore.subscribe(schedule),
    ];
    return () => {
        if (timer !== undefined) clearTimeout(timer);
        unsubscribes.forEach((unsubscribe) => unsubscribe());
    };
}

/** Any answer at all, even a refusal, means the service is up. Skips the HTTP client, which fails at once while on hold. */
export async function probeService(): Promise<void> {
    const url =
        useConfigStore.getState().config?.serviceEndpoints?.["Patient"] ??
        getConfigUrl();
    try {
        await fetch(url);
        useConnectivityStore.getState().setServiceUnavailable(false);
    } catch (error) {
        logger.debug(
            `Health Gateway still unreachable: ${describeError(error)}`
        );
    }
}
