import { create } from "zustand";

import {
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { Action, Dataset, Text } from "@/constants/tracking";
import { LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { UserNote } from "@/models/userNote";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";

import { useErrorStore } from "./error.store";
import { useTimelineStore } from "./timeline.store";
import { inProgressCalls } from "./utils/inProgressCalls";

interface NoteState {
    notes: UserNote[];
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
    retrieveNotes: (hdid: string) => Promise<void>;
    createNote: (hdid: string, note: UserNote) => Promise<void>;
    updateNote: (hdid: string, note: UserNote) => Promise<void>;
    deleteNote: (hdid: string, note: UserNote) => Promise<void>;
    reset: () => void;
}

/** EventName.UpdateTimelineEntry: the full-screen details close when they show the changed note */
function timelineEntryUpdated(id?: string) {
    const timeline = useTimelineStore.getState();
    if (id && timeline.fullscreenEntry?.id === id)
        timeline.closeFullscreenEntry();
}

/** Port of stores/note.ts (one list for the signed-in user, not per hdid). */
export const useNoteStore = create<NoteState>((set, get) => {
    const inProgress = inProgressCalls();
    function handleError(error: unknown, errorType: ErrorType) {
        logger.error(`ERROR: ${describeError(error)}`);
        const resultError = error instanceof ResultError ? error : undefined;
        set({
            error: resultError,
            statusMessage: resultError?.message ?? "",
            status: LoadStatus.ERROR,
        });
        const errorStore = useErrorStore.getState();
        if (resultError?.statusCode === 429) {
            if (errorType === ErrorType.Retrieve)
                errorStore.setTooManyRequestsWarning("page");
            else if (errorType === ErrorType.Delete)
                errorStore.setTooManyRequestsError("page");
            else
                errorStore.setTooManyRequestsError(
                    TooManyRequestsLocation.NoteDialog
                );
        } else
            errorStore.addError(
                errorType,
                ErrorSourceType.Note,
                resultError?.traceId
            );
    }

    return {
        notes: [],
        status: LoadStatus.NONE,
        statusMessage: "",
        error: undefined,
        retrieveNotes: (hdid) =>
            inProgress.share(hdid, async () => {
                if (get().status === LoadStatus.LOADED) {
                    logger.debug("Notes found stored, not querying!");
                    return;
                }
                logger.debug("Retrieving user notes");
                set({ status: LoadStatus.REQUESTED });
                try {
                    const result = await services.userNote.getNotes(hdid);
                    if (result.resultStatus === ResultType.Success) {
                        set({
                            notes: result.resourcePayload,
                            error: undefined,
                            status: LoadStatus.LOADED,
                        });
                        if (result.resourcePayload.length > 0)
                            services.tracking.trackEvent({
                                action: Action.Load,
                                text: Text.Data,
                                dataset: Dataset.Notes,
                            });
                    } else {
                        if (result.resultError)
                            throw ResultError.fromModel(result.resultError);
                        logger.warn(`Notes retrieval failed!`);
                    }
                } catch (error) {
                    rethrowIfSessionEnded(error);
                    handleError(error, ErrorType.Retrieve);
                    throw error;
                }
            }),
        createNote: async (hdid, note) => {
            try {
                const result = await services.userNote.createNote(hdid, note);
                set((s) => ({ notes: [...s.notes, result] }));
                timelineEntryUpdated(result.id);
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Create);
                throw error;
            }
        },
        updateNote: async (hdid, note) => {
            try {
                const result = await services.userNote.updateNote(hdid, note);
                const index = get().notes.findIndex((x) => x.id === result.id);
                if (index >= 0) {
                    set((s) => ({
                        notes: s.notes.map((n, i) =>
                            i === index ? result : n
                        ),
                    }));
                    timelineEntryUpdated(result.id);
                }
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Update);
                throw error;
            }
        },
        deleteNote: async (hdid, note) => {
            try {
                await services.userNote.deleteNote(hdid, note);
                const index = get().notes.findIndex((x) => x.id === note.id);
                if (index >= 0) {
                    set((s) => ({
                        notes: s.notes.filter((_, i) => i !== index),
                    }));
                    timelineEntryUpdated(note.id);
                }
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Delete);
                throw error;
            }
        },
        reset: () => {
            inProgress.clear();
            set({
                notes: [],
                status: LoadStatus.NONE,
                statusMessage: "",
                error: undefined,
            });
        },
    };
});

export const selectNotesAreLoading = (s: NoteState) =>
    s.status === LoadStatus.REQUESTED;

/** The note dialog's open request (EventName.OpenNoteDialog): undefined = closed, null = a new note. */
interface NoteDialogState {
    open: boolean;
    entryId?: string;
    openDialog: (entryId?: string) => void;
    closeDialog: () => void;
}
export const useNoteDialogStore = create<NoteDialogState>((set) => ({
    open: false,
    entryId: undefined,
    openDialog: (entryId) => set({ open: true, entryId }),
    closeDialog: () => set({ open: false }),
}));
