import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserNoteService } from "@/services/interfaces";

import { NoteActions } from "./types";

export const actions: NoteActions = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<UserNote[]>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );

        return new Promise((resolve, reject) => {
            const userNotes: UserNote[] = context.getters.notes;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Notes found stored, not querying!`);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: userNotes,
                    resultStatus: ResultType.Success,
                    totalResultCount: userNotes.length,
                });
            } else {
                logger.debug(`Retrieving User notes`);
                context.commit("setRequested");
                noteService
                    .getNotes(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", {
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        } else {
                            context.commit("setNotes", result.resourcePayload);
                            resolve(result);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    createNote(
        context,
        params: { hdid: string; note: UserNote }
    ): Promise<UserNote | undefined> {
        const noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );

        return new Promise((resolve, reject) =>
            noteService
                .createNote(params.hdid, params.note)
                .then((result) => {
                    context.commit("addNote", result);
                    resolve(result);
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Create,
                    });
                    reject(error);
                })
        );
    },
    updateNote(
        context,
        params: { hdid: string; note: UserNote }
    ): Promise<UserNote> {
        const noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );

        return new Promise<UserNote>((resolve, reject) =>
            noteService
                .updateNote(params.hdid, params.note)
                .then((result) => {
                    context.commit("updateNote", result);
                    resolve(result);
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Update,
                    });
                    reject(error);
                })
        );
    },
    deleteNote(
        context,
        params: { hdid: string; note: UserNote }
    ): Promise<void> {
        const noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );

        return new Promise((resolve, reject) =>
            noteService
                .deleteNote(params.hdid, params.note)
                .then(() => {
                    context.commit("deleteNote", params.note);
                    resolve();
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Delete,
                    });
                    reject(error);
                })
        );
    },
    handleError(context, params: { error: ResultError; errorType: ErrorType }) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("noteError", params.error);
        context.dispatch(
            "errorBanner/addError",
            {
                errorType: params.errorType,
                source: ErrorSourceType.Note,
                traceId: params.error.traceId,
            },
            { root: true }
        );
    },
};
