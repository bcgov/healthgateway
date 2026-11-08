import { AppState } from "react-native";

import { DataSource, getDataSource } from "@/constants/dataSource";
import {
    EntryType,
    entryTypes as allEntryTypes,
    ServiceName,
} from "@/constants/entryType";
import { LoadStatus } from "@/models/datasetState";
import { PatientDataType } from "@/models/patientData";
import { isFullyRegisteredUser } from "@/navigation/userState";
import { describeError, logger } from "@/services/logger";
import { entryTypeToPatientDataTypeMap } from "@/services/patientData.service";
import { COPY_DECLINED, recordCache } from "@/services/recordCache";
import { sessionGeneration } from "@/services/sessionFence";
import { ConfigUtil } from "@/utils/configUtil";
import { isDependentExpired } from "@/utils/dependentAge";

import { useAppLockStore } from "./appLock.store";
import { refreshTokenIfDue, useAuthStore } from "./auth.store";
import { useClinicalDocumentStore } from "./clinicalDocument.store";
import { useCommentStore } from "./comment.store";
import {
    selectFeatureToggles,
    selectWebConfig,
    useConfigStore,
} from "./config.store";
import { selectIsLive, useConnectivityStore } from "./connectivity.store";
import { useCovid19TestResultStore } from "./covid19TestResult.store";
import { useDependentStore } from "./dependent.store";
import { useHealthVisitStore } from "./healthVisit.store";
import { useHospitalVisitStore } from "./hospitalVisit.store";
import { useImmunizationStore } from "./immunization.store";
import { useLabResultStore } from "./labResult.store";
import {
    forgetFromLastCopy,
    seedRecords,
    type SnapshotData,
} from "./localSession";
import { useMedicationStore } from "./medication.store";
import { useNoteStore } from "./note.store";
import { usePatientDataStore } from "./patientData.store";
import { clearDatasetRecords, markDatasetStale } from "./refreshDatasets";
import {
    allowDataset,
    clearDatasetFailures,
    datasetFailedAt,
    datasetKey,
    datasetRefreshedAt,
    forgetDataset,
    isDatasetCleared,
    markDatasetFailed,
    markDatasetRefreshed,
    REFRESH_AFTER_MS,
    resetSavedCopyState,
    setCopyReadFor,
    setCopyReadRetry,
} from "./savedCopyState";
import { useSpecialAuthorityRequestStore } from "./specialAuthorityRequest.store";
import { useUserStore } from "./user.store";

/** After a data API failed. */
const RETRY_FAILED_MS = 5 * 60 * 1000;
/** How often an open app checks that. */
const AGE_CHECK_MS = 5 * 60 * 1000;
/** As long as the Refreshing pill: says it started, without making anyone wait for it. */
const PILL_MS = 1200;
/** Failures stay quiet until this at most; anything slower (lab results can take minutes) still arrives and is saved. */
const QUIET_MAX_MS = 180000;

const perPerson =
    <S>(store: { getState: () => { states: Record<string, S> } }) =>
    (hdid: string) =>
        store.getState().states[hdid];

const retrievers: Partial<
    Record<
        EntryType,
        {
            /** its name in the saved copy */
            name: string;
            retrieve: (hdid: string) => Promise<unknown>;
            state: (hdid: string) => { status?: LoadStatus } | undefined;
        }
    >
> = {
    [EntryType.Immunization]: {
        name: "immunization",
        retrieve: (h) =>
            useImmunizationStore.getState().retrieveImmunizations(h),
        state: perPerson(useImmunizationStore),
    },
    [EntryType.Medication]: {
        name: "medication",
        retrieve: (h) => useMedicationStore.getState().retrieveMedications(h),
        state: perPerson(useMedicationStore),
    },
    [EntryType.LabResult]: {
        name: "labResult",
        retrieve: (h) => useLabResultStore.getState().retrieveLabResults(h),
        state: perPerson(useLabResultStore),
    },
    [EntryType.Covid19TestResult]: {
        name: "covid19TestResult",
        retrieve: (h) =>
            useCovid19TestResultStore.getState().retrieveCovid19TestResults(h),
        state: perPerson(useCovid19TestResultStore),
    },
    [EntryType.HealthVisit]: {
        name: "healthVisit",
        retrieve: (h) => useHealthVisitStore.getState().retrieve(h),
        state: perPerson(useHealthVisitStore),
    },
    [EntryType.Note]: {
        name: "notes",
        retrieve: (h) => useNoteStore.getState().retrieveNotes(h),
        state: () => useNoteStore.getState(),
    },
    [EntryType.SpecialAuthorityRequest]: {
        name: "specialAuthorityRequest",
        retrieve: (h) =>
            useSpecialAuthorityRequestStore
                .getState()
                .retrieveSpecialAuthorityRequests(h),
        state: perPerson(useSpecialAuthorityRequestStore),
    },
    [EntryType.ClinicalDocument]: {
        name: "clinicalDocument",
        retrieve: (h) => useClinicalDocumentStore.getState().retrieve(h),
        state: perPerson(useClinicalDocumentStore),
    },
    [EntryType.HospitalVisit]: {
        name: "hospitalVisit",
        retrieve: (h) =>
            useHospitalVisitStore.getState().retrieveHospitalVisits(h),
        state: perPerson(useHospitalVisitStore),
    },
};

/**
 * Whether a dataset really came back. Fetches can finish without throwing and still not have records: a failed result, or
 * lab results still being processed. Protected medications count: only the user's word can open them, so no refresh could.
 */
const isComplete = (state: { status?: LoadStatus } | undefined) =>
    state?.status === LoadStatus.LOADED ||
    state?.status === LoadStatus.PROTECTED;

/** Due: no refresh that finished, or an old one, and not one that failed moments ago. */
const isDue = (key: string) => {
    if (isDatasetCleared(key)) return false;
    const now = Date.now();
    const refreshedAt = datasetRefreshedAt(key);
    if (refreshedAt !== undefined && now - refreshedAt < REFRESH_AFTER_MS)
        return false;
    const failedAt = datasetFailedAt(key);
    return failedAt === undefined || now - failedAt >= RETRY_FAILED_MS;
};

/** One data API's refresh, when it is due. One failure stops nothing else, and says nothing to the user: opening the page tries again. */
async function refresh(
    person: string,
    name: string,
    retrieve: () => Promise<unknown>,
    finished: () => { status?: LoadStatus } | undefined,
    force = false
): Promise<void> {
    const key = datasetKey(person, name);
    if (!force && !isDue(key)) return;
    markDatasetStale(name, person);
    try {
        await retrieve();
    } catch (error) {
        logger.debug(`Background sync: ${describeError(error)}`);
    }
    if (isComplete(finished())) markDatasetRefreshed(key, Date.now());
    else markDatasetFailed(key, Date.now());
}

const allowedTypes = (entryTypes: EntryType[]) => {
    const { blockedDataSources } = useUserStore.getState().user;
    return entryTypes.filter(
        (t) => !blockedDataSources.includes(getDataSource(t))
    );
};

const patientDataFor = (
    allowed: EntryType[],
    extraPatientData: PatientDataType[]
) => [
    ...allowed.flatMap((t) => {
        const type = entryTypeToPatientDataTypeMap.get(t);
        return type ? [type] : [];
    }),
    ...extraPatientData,
];

/** The same rule as the timeline's first load, for one person. */
async function retrievePerson(
    hdid: string,
    entryTypes: EntryType[],
    extraPatientData: PatientDataType[] = []
): Promise<void> {
    const allowed = allowedTypes(entryTypes);
    const patientData = patientDataFor(allowed, extraPatientData);
    await Promise.all([
        ...allowed.flatMap((t) => {
            const dataset = retrievers[t];
            return dataset
                ? [
                      refresh(
                          hdid,
                          dataset.name,
                          () => dataset.retrieve(hdid),
                          () => dataset.state(hdid)
                      ),
                  ]
                : [];
        }),
        ...(patientData.length > 0
            ? [
                  refresh(
                      hdid,
                      "patientData",
                      () =>
                          usePatientDataStore
                              .getState()
                              .retrievePatientData(hdid, patientData),
                      () => usePatientDataStore.getState().states[hdid]
                  ),
              ]
            : []),
    ]);
}

const dependentTypes = () => {
    const toggles = selectFeatureToggles(useConfigStore.getState())?.dependents;
    return (
        toggles?.timelineEnabled
            ? allEntryTypes
                  .map((d) => d.type)
                  .filter((t) => t !== EntryType.Note)
            : [
                  EntryType.Immunization,
                  EntryType.LabResult,
                  EntryType.ClinicalDocument,
              ]
    ).filter((t) => ConfigUtil.isDependentDatasetEnabled(t));
};

const dependentsEnabled = () =>
    selectFeatureToggles(useConfigStore.getState())?.dependents?.enabled ??
    false;

const currentDependents = () => {
    const maxAge =
        selectWebConfig(useConfigStore.getState())?.maxDependentAge ?? 0;
    return useDependentStore
        .getState()
        .dependents.filter((d) => !isDependentExpired(d, maxAge));
};

async function retrieveDependents(hdid: string): Promise<void> {
    if (!dependentsEnabled()) return;
    await refresh(
        hdid,
        "dependents",
        () => useDependentStore.getState().retrieveDependents(hdid, false),
        () => useDependentStore.getState()
    );
    const types = dependentTypes();
    // one at a time: someone with several dependents should not hit the rate limit
    for (const dependent of currentDependents())
        await retrievePerson(dependent.ownerId, types);
}

const selfTypes = () =>
    allEntryTypes
        .map((d) => d.type)
        .filter((t) => ConfigUtil.isDatasetEnabled(t));

const organDonorData = () =>
    ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
    !useUserStore
        .getState()
        .user.blockedDataSources.includes(DataSource.OrganDonorRegistration)
        ? [PatientDataType.OrganDonorRegistrationStatus]
        : [];

const commentsEnabled = () =>
    selectWebConfig(useConfigStore.getState())?.featureToggleConfiguration
        .timeline.comment ?? false;

async function syncEverything(hdid: string): Promise<void> {
    await Promise.all([
        retrievePerson(hdid, selfTypes(), organDonorData()),
        ...(commentsEnabled()
            ? [
                  refresh(
                      hdid,
                      "comments",
                      () => useCommentStore.getState().retrieveComments(hdid),
                      () => useCommentStore.getState()
                  ),
              ]
            : []),
        retrieveDependents(hdid),
    ]);
}

export interface CachedDataset {
    key: string;
    person: string;
    name: string;
}

/** One person's data APIs, by their names in the saved copy. */
const personDatasets = (
    person: string,
    entryTypes: EntryType[],
    extraPatientData: PatientDataType[] = []
): CachedDataset[] => {
    const allowed = allowedTypes(entryTypes);
    const names = allowed.flatMap((t) => {
        const name = retrievers[t]?.name;
        return name ? [name] : [];
    });
    if (patientDataFor(allowed, extraPatientData).length > 0)
        names.push("patientData");
    return names.map((name) => ({
        key: datasetKey(person, name),
        person,
        name,
    }));
};

/** Every data API the copy should hold for this login. */
export const cachedDatasets = (hdid: string): CachedDataset[] => [
    ...personDatasets(hdid, selfTypes(), organDonorData()),
    ...(commentsEnabled()
        ? [
              {
                  key: datasetKey(hdid, "comments"),
                  person: hdid,
                  name: "comments",
              },
          ]
        : []),
    ...(dependentsEnabled()
        ? [
              {
                  key: datasetKey(hdid, "dependents"),
                  person: hdid,
                  name: "dependents",
              },
              ...currentDependents().flatMap((d) =>
                  personDatasets(d.ownerId, dependentTypes())
              ),
          ]
        : []),
];

const allKeys = (hdid: string) => cachedDatasets(hdid).map((d) => d.key);

/** A dataset's fetch and how to tell it finished, for a refresh asked for by hand. */
const refreshByHand = (hdid: string, { person, name }: CachedDataset) => {
    if (name === "comments")
        return refresh(
            person,
            name,
            () => useCommentStore.getState().retrieveComments(person),
            () => useCommentStore.getState(),
            true
        );
    if (name === "dependents")
        return refresh(
            person,
            name,
            () =>
                useDependentStore.getState().retrieveDependents(person, false),
            () => useDependentStore.getState(),
            true
        );
    if (name === "patientData")
        return refresh(
            person,
            name,
            () =>
                usePatientDataStore
                    .getState()
                    .retrievePatientData(
                        person,
                        person === hdid
                            ? patientDataFor(
                                  allowedTypes(selfTypes()),
                                  organDonorData()
                              )
                            : patientDataFor(allowedTypes(dependentTypes()), [])
                    ),
            () => usePatientDataStore.getState().states[person],
            true
        );
    const dataset = Object.values(retrievers).find((r) => r.name === name);
    return dataset
        ? refresh(
              person,
              name,
              () => dataset.retrieve(person),
              () => dataset.state(person),
              true
          )
        : Promise.resolve();
};

let refreshesByHand = 0;
/** Failures stay quiet, as in the background: the Security page shows them against the dataset. */
async function quietly(work: () => Promise<unknown>): Promise<void> {
    refreshesByHand += 1;
    useConnectivityStore.getState().setSyncing(true);
    try {
        await work();
    } finally {
        refreshesByHand -= 1;
        // this change also saves the copy
        if (refreshesByHand === 0)
            useConnectivityStore.getState().setSyncing(false);
    }
}

/** Security page: fetches one data API now, whatever its age. */
export async function resyncDataset(
    hdid: string,
    dataset: CachedDataset
): Promise<void> {
    allowDataset(dataset.key);
    await quietly(() => refreshByHand(hdid, dataset));
}

/** Security page: fetches everything now, whatever its age. One person at a time, as in the background. */
export async function resyncAllDatasets(hdid: string): Promise<void> {
    await quietly(async () => {
        const dependentsList = cachedDatasets(hdid).filter(
            (d) => d.name === "dependents"
        );
        for (const dataset of dependentsList) {
            allowDataset(dataset.key);
            await refreshByHand(hdid, dataset);
        }
        const datasets = cachedDatasets(hdid).filter(
            (d) => d.name !== "dependents"
        );
        for (const person of new Set(datasets.map((d) => d.person)))
            await Promise.all(
                datasets
                    .filter((d) => d.person === person)
                    .map((dataset) => {
                        allowDataset(dataset.key);
                        return refreshByHand(hdid, dataset);
                    })
            );
    });
}

/** Security page: removes one data API's records until the next launch or a resync. Opening its page still fetches it. */
export function clearDataset(dataset: CachedDataset): void {
    clearDatasetRecords(dataset.name, dataset.person);
    forgetFromLastCopy(dataset.person, dataset.name);
    forgetDataset(dataset.key);
}

/** Security page: removes this person's records and saved copy. Anyone else's stays: only Reset App removes those. */
export async function clearAllDatasets(hdid: string): Promise<void> {
    cachedDatasets(hdid).forEach(clearDataset);
    await recordCache.removeFor(hdid);
}

/**
 * Native, with device security: the saved copy is what the app shows. It opens straight away on launch, and each data API
 * is only fetched again, in the background, when it has no finished copy or one older than `REFRESH_AFTER_MS`. One that
 * fails is retried on its own after `RETRY_FAILED_MS`, and doesn't hold back the rest. Otherwise new records come from
 * pulling to refresh or the Security page. Logging out keeps the copy for that person's next login; another person logging in removes it once expired.
 * Reading it may make the OS ask. Turned down, the first save asks again, as it can't overwrite an unread copy; turned down
 * twice, this login has no saved copy: nothing here runs, and pages fetch into their stores as they would without device security.
 */
export function startFirstRecordSync(): () => void {
    let loadedFor: string | undefined;
    let running = false;
    let gaveUp = false;
    let pillTimer: ReturnType<typeof setTimeout> | undefined;
    let quietTimer: ReturnType<typeof setTimeout> | undefined;
    let nextAttemptAt = 0;
    let copyDeclines = 0;
    /** Waiting for the first save to ask again, or for good once that was turned down too. */
    let copyDeclined = false;

    const run = async (hdid: string) => {
        running = true;
        const session = sessionGeneration();
        const ended = () => session !== sessionGeneration();
        if (loadedFor !== hdid) {
            const copy = await recordCache.load<SnapshotData>(hdid);
            if (ended()) return;
            if (copy === COPY_DECLINED) {
                running = false;
                copyDeclined = true;
                copyDeclines += 1;
                if (copyDeclines === 1)
                    setCopyReadRetry(() => {
                        copyDeclined = false;
                        check();
                    });
                return;
            }
            if (copy) seedRecords(hdid, copy.data);
            else resetSavedCopyState();
            setCopyReadFor(hdid);
            loadedFor = hdid;
            void recordCache.removeExpired(hdid, Date.now());
        }
        const due = allKeys(hdid).filter(isDue);
        if (due.length === 0) {
            running = false;
            return;
        }
        // back from the background, the access token may be out of date
        await refreshTokenIfDue();
        if (ended()) return;
        useConnectivityStore.getState().setSyncing(true);
        // a retry of something that failed is quiet: it isn't news, and it would otherwise flash on every launch
        if (due.some((key) => datasetFailedAt(key) === undefined)) {
            useConnectivityStore.setState({ syncPill: true });
            pillTimer = setTimeout(
                () => useConnectivityStore.setState({ syncPill: false }),
                PILL_MS
            );
        }
        const finished = await Promise.race([
            syncEverything(hdid).then(() => true),
            new Promise<false>((resolve) => {
                quietTimer = setTimeout(() => resolve(false), QUIET_MAX_MS);
            }),
        ]);
        if (ended()) return;
        clearTimeout(quietTimer);
        const live = selectIsLive(useConnectivityStore.getState());
        // Too slow: the next launch finishes the job, and what comes back meanwhile still counts
        if (!finished) gaveUp = true;
        // Cut off by losing the connection: says nothing about the data APIs, so tries when it is back
        else if (!live) clearDatasetFailures();
        else if (
            allKeys(hdid).some((key) => datasetFailedAt(key) !== undefined)
        )
            nextAttemptAt = Date.now() + RETRY_FAILED_MS;
        running = false;
        // this change also saves the copy
        useConnectivityStore.getState().setSyncing(false);
    };

    const check = () => {
        const { essentialDataLoaded, user } = useUserStore.getState();
        if (!useAuthStore.getState().isAuthenticated) {
            // whatever was running for that login stops at its next step, or ends as cancelled
            clearTimeout(pillTimer);
            clearTimeout(quietTimer);
            running = false;
            // each change calls this again straight away, so only make one that is still needed
            if (useConnectivityStore.getState().syncPill)
                useConnectivityStore.setState({ syncPill: false });
            if (useConnectivityStore.getState().syncing)
                useConnectivityStore.getState().setSyncing(false);
            loadedFor = undefined;
            gaveUp = false;
            nextAttemptAt = 0;
            copyDeclines = 0;
            copyDeclined = false;
            resetSavedCopyState();
            return;
        }
        const lock = useAppLockStore.getState();
        if (
            running ||
            gaveUp ||
            copyDeclined ||
            // a prompt can't open from there, and would count as turned down
            AppState.currentState === "background" ||
            Date.now() < nextAttemptAt ||
            !user.hdid ||
            !essentialDataLoaded ||
            !lock.enabled ||
            lock.locked ||
            !isFullyRegisteredUser() ||
            !selectIsLive(useConnectivityStore.getState())
        )
            return;
        void run(user.hdid);
    };

    const unsubscribes = [
        useAuthStore.subscribe(check),
        useUserStore.subscribe(check),
        useAppLockStore.subscribe(check),
        useConnectivityStore.subscribe(check),
    ];
    const foreground = AppState.addEventListener("change", (state) => {
        if (state === "active") check();
    });
    // still open when the copy turns old
    const clock = setInterval(check, AGE_CHECK_MS);
    check();
    return () => {
        clearInterval(clock);
        clearTimeout(pillTimer);
        clearTimeout(quietTimer);
        foreground.remove();
        unsubscribes.forEach((unsubscribe) => unsubscribe());
    };
}
