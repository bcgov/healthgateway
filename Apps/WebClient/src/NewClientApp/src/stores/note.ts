import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { LoadStatus, Operation, OperationType } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import { ILogger, IUserNoteService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

export const useNoteStore = defineStore("note", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const noteService = container.get<IUserNoteService>(
        SERVICE_IDENTIFIER.NoteService
    );

    const errorStore = useErrorStore();

    // Refs
    const notes = ref<UserNote[]>([]);
    const status = ref(LoadStatus.NONE);
    const statusMessage = ref("");
    const error = ref<ResultError>();
    const lastOperation = ref<Operation>();

    // Computed
    const notesCount = computed(() => notes.value.length);

    const noteAreLoading = computed(
        () => status.value === LoadStatus.REQUESTED
    );

    // Mutations
    function setNotesRequested() {
        status.value = LoadStatus.REQUESTED;
    }

    function setNotesLoaded(userNotes: UserNote[]) {
        notes.value = userNotes;
        error.value = undefined;
        status.value = LoadStatus.LOADED;
    }

    function commitCreateNote(note: UserNote) {
        lastOperation.value = new Operation(
            note.id as string,
            OperationType.ADD
        );
        notes.value.push(note);
    }

    function commitUpdateNote(note: UserNote) {
        lastOperation.value = new Operation(
            note.id as string,
            OperationType.UPDATE
        );
    }

    function commitDeleteNote(note: UserNote) {
        const noteIndex = notes.value.findIndex((x) => x.id === note.id);
        if (noteIndex > -1) {
            lastOperation.value = new Operation(
                note.id as string,
                OperationType.DELETE
            );
            notes.value.splice(noteIndex, 1);
        }
    }

    function setNotesError(resultError: ResultError) {
        error.value = resultError;
        statusMessage.value = resultError.resultMessage;
        status.value = LoadStatus.ERROR;
    }

    // Helpers
    function handleError(resultError: ResultError, errorType: ErrorType): void {
        logger.error(`ERROR: ${JSON.stringify(resultError)}`);
        setNotesError(resultError);

        if (resultError.statusCode === 429) {
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning("page");
            } else if (errorType === ErrorType.Delete) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.setTooManyRequestsError("noteEditModal");
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Note,
                resultError.traceId
            );
        }
    }

    // Actions
    function retrieveNotes(hdid: string): Promise<void> {
        if (status.value === LoadStatus.LOADED) {
            logger.debug(`Notes found stored, not querying!`);
            return Promise.resolve();
        } else {
            logger.debug(`Retrieving User notes`);
            setNotesRequested();
            return noteService
                .getNotes(hdid)
                .then((result) => {
                    if (result.resultStatus === ResultType.Success) {
                        setNotesLoaded(result.resourcePayload);
                    } else {
                        if (result.resultError) {
                            throw result.resultError;
                        }
                        logger.warn(
                            `Notes retrieval failed! ${JSON.stringify(result)}`
                        );
                    }
                })
                .catch((error: ResultError) => {
                    handleError(error, ErrorType.Retrieve);
                    throw error;
                });
        }
    }

    function createNote(
        hdid: string,
        note: UserNote
    ): Promise<UserNote | undefined> {
        return noteService
            .createNote(hdid, note)
            .then((result) => {
                if (result !== undefined) {
                    commitCreateNote(result);
                } else {
                    logger.debug(`Note creation returned undefined!`);
                }
                return result;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Create);
                throw error;
            });
    }

    function updateNote(hdid: string, note: UserNote): Promise<UserNote> {
        return noteService
            .updateNote(hdid, note)
            .then((result) => {
                commitUpdateNote(result);
                return result;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Update);
                throw error;
            });
    }

    function deleteNote(hdid: string, note: UserNote): Promise<void> {
        return noteService
            .deleteNote(hdid, note)
            .then(() => {
                commitDeleteNote(note);
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Delete);
                throw error;
            });
    }

    return {
        lastOperation,
        notesCount,
        noteAreLoading,
        retrieveNotes,
        createNote,
        updateNote,
        deleteNote,
    };
});
