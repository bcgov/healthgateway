import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import { ILogger, IUserNoteService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { EventName, useEventStore } from "@/stores/event";
import EventTracker from "@/utility/eventTracker";

export const useNoteStore = defineStore("note", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const noteService = container.get<IUserNoteService>(
        SERVICE_IDENTIFIER.UserNoteService
    );
    const errorStore = useErrorStore();
    const eventStore = useEventStore();

    const notes = ref<UserNote[]>([]);
    const status = ref(LoadStatus.NONE);
    const statusMessage = ref("");
    const error = ref<ResultError>();

    const notesCount = computed(() => notes.value.length);
    const notesAreLoading = computed(
        () => status.value === LoadStatus.REQUESTED
    );

    function handleError(resultError: ResultError, errorType: ErrorType): void {
        logger.error(`ERROR: ${JSON.stringify(resultError)}`);
        error.value = resultError;
        statusMessage.value = resultError.resultMessage;
        status.value = LoadStatus.ERROR;

        if (resultError.statusCode === 429) {
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning("page");
            } else if (errorType === ErrorType.Delete) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.setTooManyRequestsError("noteDialog");
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Note,
                resultError.traceId
            );
        }
    }

    function retrieveNotes(hdid: string): Promise<void> {
        if (status.value === LoadStatus.LOADED) {
            logger.debug(`Notes found stored, not querying!`);
            return Promise.resolve();
        } else {
            logger.debug(`Retrieving user notes`);
            status.value = LoadStatus.REQUESTED;
            return noteService
                .getNotes(hdid)
                .then((result) => {
                    EventTracker.loadData(
                        EntryType.Note,
                        result.resourcePayload.length
                    );
                    if (result.resultStatus === ResultType.Success) {
                        notes.value = result.resourcePayload;
                        error.value = undefined;
                        status.value = LoadStatus.LOADED;
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

    function createNote(hdid: string, note: UserNote): Promise<void> {
        return noteService
            .createNote(hdid, note)
            .then((result) => {
                notes.value.push(result);
                eventStore.emit(EventName.UpdateTimelineEntry, result.id);
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Create);
                throw error;
            });
    }

    function updateNote(hdid: string, note: UserNote): Promise<void> {
        return noteService
            .updateNote(hdid, note)
            .then((result) => {
                const index = notes.value.findIndex((x) => x.id === result.id);
                if (index >= 0) {
                    notes.value[index] = result;
                    eventStore.emit(EventName.UpdateTimelineEntry, result.id);
                }
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
                const index = notes.value.findIndex((x) => x.id === note.id);
                if (index >= 0) {
                    notes.value.splice(index, 1);
                    eventStore.emit(EventName.UpdateTimelineEntry, note.id);
                }
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Delete);
                throw error;
            });
    }

    return {
        notes,
        notesCount,
        notesAreLoading,
        retrieveNotes,
        createNote,
        updateNote,
        deleteNote,
    };
});
