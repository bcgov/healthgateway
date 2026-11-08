import { LoadStatus } from "@/models/datasetState";

import { useClinicalDocumentStore } from "./clinicalDocument.store";
import { useCommentStore } from "./comment.store";
import { useCovid19TestResultStore } from "./covid19TestResult.store";
import { useDependentStore } from "./dependent.store";
import { useHealthVisitStore } from "./healthVisit.store";
import { useHospitalVisitStore } from "./hospitalVisit.store";
import { useImmunizationStore } from "./immunization.store";
import { useLabResultStore } from "./labResult.store";
import { useMedicationStore } from "./medication.store";
import { useNoteStore } from "./note.store";
import { usePatientDataStore } from "./patientData.store";
import { useSpecialAuthorityRequestStore } from "./specialAuthorityRequest.store";

function stale<S extends { status: LoadStatus }>(
    states: Record<string, S>,
    hdid: string
): Record<string, S> {
    const state = states[hdid];
    if (state?.status !== LoadStatus.LOADED) return states;
    return { ...states, [hdid]: { ...state, status: LoadStatus.NONE } };
}

/**
 * Pull to refresh: the stores skip a dataset that is LOADED, so this makes their next retrieve fetch again. Each keeps
 * its records until the new ones arrive, so a failed fetch leaves them on screen behind the error banner.
 *
 * Medications opened with a protective word stay as they are: fetching again would drop them and ask for the word.
 */
export function markTimelineDatasetsStale(hdid: string): void {
    useImmunizationStore.setState((s) => ({ states: stale(s.states, hdid) }));
    useLabResultStore.setState((s) => ({ states: stale(s.states, hdid) }));
    useCovid19TestResultStore.setState((s) => ({
        states: stale(s.states, hdid),
    }));
    useHealthVisitStore.setState((s) => ({ states: stale(s.states, hdid) }));
    useSpecialAuthorityRequestStore.setState((s) => ({
        states: stale(s.states, hdid),
    }));
    useClinicalDocumentStore.setState((s) => ({
        states: stale(s.states, hdid),
    }));
    useHospitalVisitStore.setState((s) => ({ states: stale(s.states, hdid) }));
    usePatientDataStore.setState((s) => ({ states: stale(s.states, hdid) }));
    useMedicationStore.setState((s) =>
        (s.states[hdid]?.protectiveWordAttempts ?? 0) > 0
            ? s
            : { states: stale(s.states, hdid) }
    );
    if (useNoteStore.getState().status === LoadStatus.LOADED)
        useNoteStore.setState({ status: LoadStatus.NONE });
    if (useCommentStore.getState().status === LoadStatus.LOADED)
        useCommentStore.setState({ status: LoadStatus.NONE });
}

/** Drops a loaded or failed dataset, so its next retrieve fetches from empty. One still being fetched is left to finish. */
function cleared<S extends { status: LoadStatus }>(
    states: Record<string, S>,
    hdid: string
): Record<string, S> {
    const state = states[hdid] as S | undefined;
    if (!state || state.status === LoadStatus.REQUESTED) return states;
    const rest = { ...states };
    delete rest[hdid];
    return rest;
}

/**
 * Health Records pull to refresh: empties the timeline's datasets, so the list and its count build up again as each
 * source answers, as on a first load. A source that fails stays empty behind the error banner, as after a browser
 * refresh on web. The saved copy, if any, keeps its previous records for a dataset that comes back with none.
 *
 * Medications opened with a protective word stay as they are: fetching again would drop them and ask for the word.
 * Notes are the user's own, so they are only emptied on the user's own timeline; comments only refetch.
 */
export function clearTimelineDatasets(hdid: string, ownHdid: string): void {
    useImmunizationStore.setState((s) => ({ states: cleared(s.states, hdid) }));
    useLabResultStore.setState((s) => ({ states: cleared(s.states, hdid) }));
    useCovid19TestResultStore.setState((s) => ({
        states: cleared(s.states, hdid),
    }));
    useHealthVisitStore.setState((s) => ({ states: cleared(s.states, hdid) }));
    useSpecialAuthorityRequestStore.setState((s) => ({
        states: cleared(s.states, hdid),
    }));
    useClinicalDocumentStore.setState((s) => ({
        states: cleared(s.states, hdid),
    }));
    useHospitalVisitStore.setState((s) => ({
        states: cleared(s.states, hdid),
    }));
    usePatientDataStore.setState((s) => ({ states: cleared(s.states, hdid) }));
    useMedicationStore.setState((s) =>
        (s.states[hdid]?.protectiveWordAttempts ?? 0) > 0
            ? s
            : { states: cleared(s.states, hdid) }
    );
    const notes = useNoteStore.getState().status;
    if (
        hdid === ownHdid &&
        (notes === LoadStatus.LOADED || notes === LoadStatus.ERROR)
    )
        useNoteStore.setState({ notes: [], status: LoadStatus.NONE });
    if (useCommentStore.getState().status === LoadStatus.LOADED)
        useCommentStore.setState({ status: LoadStatus.NONE });
}

/** Services pull to refresh: the stored status stays on screen until the new one arrives. */
export function markPatientDataStale(hdid: string): void {
    usePatientDataStore.setState((s) => ({ states: stale(s.states, hdid) }));
}

/**
 * Dependents pull to refresh: the list, then whichever of each dependent's records were already opened or saved. One
 * dependent at a time, so someone with several does not hit the rate limit. Failures show as they do on the page.
 */
export async function refreshDependentsAndRecords(hdid: string): Promise<void> {
    await useDependentStore.getState().retrieveDependents(hdid, true);
    for (const { ownerId } of useDependentStore.getState().dependents) {
        const isLoaded = (state?: { status: LoadStatus }) =>
            state?.status === LoadStatus.LOADED;
        const retrievals: (() => Promise<unknown>)[] = [];
        if (isLoaded(useImmunizationStore.getState().states[ownerId])) {
            useImmunizationStore.setState((s) => ({
                states: stale(s.states, ownerId),
            }));
            retrievals.push(() =>
                useImmunizationStore.getState().retrieveImmunizations(ownerId)
            );
        }
        if (isLoaded(useLabResultStore.getState().states[ownerId])) {
            useLabResultStore.setState((s) => ({
                states: stale(s.states, ownerId),
            }));
            retrievals.push(() =>
                useLabResultStore.getState().retrieveLabResults(ownerId)
            );
        }
        if (isLoaded(useClinicalDocumentStore.getState().states[ownerId])) {
            useClinicalDocumentStore.setState((s) => ({
                states: stale(s.states, ownerId),
            }));
            retrievals.push(() =>
                useClinicalDocumentStore.getState().retrieve(ownerId)
            );
        }
        await Promise.allSettled(retrievals.map((retrieve) => retrieve()));
    }
}

const perPerson = {
    immunization: useImmunizationStore,
    labResult: useLabResultStore,
    covid19TestResult: useCovid19TestResultStore,
    healthVisit: useHealthVisitStore,
    specialAuthorityRequest: useSpecialAuthorityRequestStore,
    clinicalDocument: useClinicalDocumentStore,
    hospitalVisit: useHospitalVisitStore,
    patientData: usePatientDataStore,
    medication: useMedicationStore,
} as unknown as Record<
    string,
    {
        setState: (
            update: (s: { states: Record<string, { status: LoadStatus }> }) => {
                states: Record<string, { status: LoadStatus }>;
            }
        ) => void;
    }
>;

/** One data API's turn in a background refresh: its records stay on screen until the new ones arrive. */
export function markDatasetStale(name: string, hdid: string): void {
    if (name === "medication")
        useMedicationStore.setState((s) =>
            (s.states[hdid]?.protectiveWordAttempts ?? 0) > 0
                ? s
                : { states: stale(s.states, hdid) }
        );
    else if (name === "notes") {
        if (useNoteStore.getState().status === LoadStatus.LOADED)
            useNoteStore.setState({ status: LoadStatus.NONE });
    } else if (name === "comments") {
        if (useCommentStore.getState().status === LoadStatus.LOADED)
            useCommentStore.setState({ status: LoadStatus.NONE });
    } else if (name === "dependents") {
        if (useDependentStore.getState().status === LoadStatus.LOADED)
            useDependentStore.setState({ status: LoadStatus.NONE });
    } else
        perPerson[name]?.setState((s) => ({ states: stale(s.states, hdid) }));
}

/** A clear by hand: the records go from memory, not just from being current. */
export function clearDatasetRecords(name: string, hdid: string): void {
    if (name === "notes") useNoteStore.getState().reset();
    else if (name === "comments") useCommentStore.getState().reset();
    else if (name === "dependents") useDependentStore.getState().reset();
    else
        perPerson[name]?.setState((s) => ({
            states: Object.fromEntries(
                Object.entries(s.states).filter(([person]) => person !== hdid)
            ),
        }));
}
