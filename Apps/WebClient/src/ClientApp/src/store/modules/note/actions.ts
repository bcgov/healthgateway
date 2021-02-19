import { ActionTree, Commit } from "vuex";

import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";
import { LoadStatus, NoteState, RootState } from "@/models/storeState";
import UserNote from "@/models/userNote";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger, IUserNoteService } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`ERROR: ${JSON.stringify(error)}`);
    commit("noteError", error);
}

const noteService: IUserNoteService = container.get<IUserNoteService>(
    SERVICE_IDENTIFIER.UserNoteService
);

export const actions: ActionTree<NoteState, RootState> = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<UserNote[]>> {
        return new Promise((resolve, reject) => {
            const userNotes: UserNote[] = context.getters.notes;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Notes found stored, not quering!`);
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
                    .then((results) => {
                        context.commit("setNotes", results.resourcePayload);
                        resolve(results);
                    })
                    .catch((error) => {
                        handleError(context.commit, error);
                        reject(error);
                    });
            }
        });
    },
    createNote(
        context,
        params: { hdid: string; note: UserNote }
    ): Promise<UserNote | undefined> {
        return new Promise((resolve, reject) => {
            noteService
                .createNote(params.hdid, params.note)
                .then((result) => {
                    context.commit("addNote", result);
                    resolve(result);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    updateNote(
        context,
        params: { hdid: string; note: UserNote }
    ): Promise<UserNote> {
        return new Promise<UserNote>((resolve, reject) => {
            noteService
                .updateNote(params.hdid, params.note)
                .then((result) => {
                    context.commit("updateNote", result);
                    resolve(result);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    deleteNote(
        context,
        params: { hdid: string; note: UserNote }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            noteService
                .deleteNote(params.hdid, params.note)
                .then(() => {
                    context.commit("deleteNote", params.note);
                    resolve();
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
};
