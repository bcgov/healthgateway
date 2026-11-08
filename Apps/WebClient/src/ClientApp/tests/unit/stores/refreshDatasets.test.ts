import { afterEach, describe, expect, it } from "@jest/globals";

import { LoadStatus } from "@/models/datasetState";
import { useCommentStore } from "@/stores/comment.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import { useMedicationStore } from "@/stores/medication.store";
import { useNoteStore } from "@/stores/note.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import {
    clearTimelineDatasets,
    markTimelineDatasetsStale,
} from "@/stores/refreshDatasets";
import { resetStores } from "@/test/fixtures";

const seedImmunizations = (status: LoadStatus) =>
    useImmunizationStore.setState({
        states: {
            H: {
                data: [{ id: "kept" }],
                status,
                statusMessage: "",
                recommendations: [],
            },
        },
    } as never);

const seedMedications = (protectiveWordAttempts: number) =>
    useMedicationStore.setState({
        states: {
            H: {
                data: [{ id: "kept" }],
                status: LoadStatus.LOADED,
                statusMessage: "",
                protectiveWordAttempts,
            },
        },
    } as never);

describe("markTimelineDatasetsStale", () => {
    afterEach(() => {
        resetStores();
    });

    it("marks a loaded dataset to fetch again, keeping its records", () => {
        seedImmunizations(LoadStatus.LOADED);
        markTimelineDatasetsStale("H");
        const state = useImmunizationStore.getState().states.H;
        expect(state.status).toBe(LoadStatus.NONE);
        expect(state.data).toEqual([{ id: "kept" }]);
    });

    it("leaves a dataset that is still loading alone", () => {
        seedImmunizations(LoadStatus.REQUESTED);
        markTimelineDatasetsStale("H");
        expect(useImmunizationStore.getState().states.H.status).toBe(
            LoadStatus.REQUESTED
        );
    });

    it("leaves another person's records alone", () => {
        seedImmunizations(LoadStatus.LOADED);
        markTimelineDatasetsStale("OTHER");
        expect(useImmunizationStore.getState().states.H.status).toBe(
            LoadStatus.LOADED
        );
    });

    it("refreshes medications, unless a protective word opened them", () => {
        seedMedications(0);
        markTimelineDatasetsStale("H");
        expect(useMedicationStore.getState().states.H.status).toBe(
            LoadStatus.NONE
        );

        seedMedications(1);
        markTimelineDatasetsStale("H");
        expect(useMedicationStore.getState().states.H.status).toBe(
            LoadStatus.LOADED
        );
    });

    it("covers notes, comments and patient data", () => {
        useNoteStore.setState({ status: LoadStatus.LOADED });
        useCommentStore.setState({ status: LoadStatus.LOADED });
        usePatientDataStore.setState({
            states: {
                H: { data: {}, status: LoadStatus.LOADED, statusMessage: "" },
            },
        });
        markTimelineDatasetsStale("H");
        expect(useNoteStore.getState().status).toBe(LoadStatus.NONE);
        expect(useCommentStore.getState().status).toBe(LoadStatus.NONE);
        expect(usePatientDataStore.getState().states.H.status).toBe(
            LoadStatus.NONE
        );
    });
});

describe("clearTimelineDatasets", () => {
    afterEach(() => {
        resetStores();
    });

    it.each([LoadStatus.LOADED, LoadStatus.ERROR])(
        "empties a finished dataset (%s), so it loads again from nothing",
        (status) => {
            seedImmunizations(status);
            clearTimelineDatasets("H", "H");
            expect(useImmunizationStore.getState().states.H).toBeUndefined();
        }
    );

    it("leaves a dataset that is still loading to finish", () => {
        seedImmunizations(LoadStatus.REQUESTED);
        clearTimelineDatasets("H", "H");
        expect(useImmunizationStore.getState().states.H.data).toEqual([
            { id: "kept" },
        ]);
    });

    it("leaves another person's records alone", () => {
        seedImmunizations(LoadStatus.LOADED);
        clearTimelineDatasets("OTHER", "H");
        expect(useImmunizationStore.getState().states.H.status).toBe(
            LoadStatus.LOADED
        );
    });

    it("keeps medications a protective word opened", () => {
        seedMedications(1);
        clearTimelineDatasets("H", "H");
        expect(useMedicationStore.getState().states.H.data).toEqual([
            { id: "kept" },
        ]);
        seedMedications(0);
        clearTimelineDatasets("H", "H");
        expect(useMedicationStore.getState().states.H).toBeUndefined();
    });

    it("empties notes only on the user's own timeline; comments only refetch", () => {
        const seed = () => {
            useNoteStore.setState({
                notes: [{ id: "n" }] as never,
                status: LoadStatus.LOADED,
            });
            useCommentStore.setState({ status: LoadStatus.LOADED });
        };
        seed();
        clearTimelineDatasets("DEPENDENT", "H");
        expect(useNoteStore.getState().notes).toHaveLength(1);

        seed();
        clearTimelineDatasets("H", "H");
        expect(useNoteStore.getState().notes).toEqual([]);
        expect(useNoteStore.getState().status).toBe(LoadStatus.NONE);
        expect(useCommentStore.getState().status).toBe(LoadStatus.NONE);
    });
});
