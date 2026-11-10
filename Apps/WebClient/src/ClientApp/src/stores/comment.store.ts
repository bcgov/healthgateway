import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { UserComment } from "@/models/userComment";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { ascending, fromIso } from "@/utils/dateUtil";

import { useErrorStore } from "./error.store";
import { inProgressCalls } from "./utils/inProgressCalls";

const commentsSort = (a: UserComment, b: UserComment) =>
    ascending(fromIso(a.createdDateTime), fromIso(b.createdDateTime));

interface CommentState {
    /** comments per parent entry id */
    comments: Record<string, UserComment[]>;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
    retrieveComments: (hdid: string) => Promise<void>;
    createComment: (
        hdid: string,
        comment: UserComment
    ) => Promise<UserComment | undefined>;
    updateComment: (hdid: string, comment: UserComment) => Promise<UserComment>;
    deleteComment: (hdid: string, comment: UserComment) => Promise<void>;
    reset: () => void;
}

/** Port of stores/comment.ts (all of the user's comments, keyed by entry). */
export const useCommentStore = create<CommentState>((set, get) => {
    const inProgress = inProgressCalls();
    function handleError(error: unknown, errorType: ErrorType) {
        logger.error(`ERROR: ${describeError(error)}`);
        const resultError = error instanceof ResultError ? error : undefined;
        set({
            error: resultError,
            status: LoadStatus.ERROR,
            statusMessage: resultError?.message ?? "",
        });
        const errorStore = useErrorStore.getState();
        if (resultError?.statusCode === 429) {
            if (errorType === ErrorType.Retrieve)
                errorStore.setTooManyRequestsWarning("page");
            else errorStore.setTooManyRequestsError("page");
        } else
            errorStore.addError(
                errorType,
                ErrorSourceType.Comment,
                resultError?.traceId
            );
    }

    const withEntry = (
        entryId: string,
        change: (list: UserComment[]) => UserComment[]
    ) =>
        set((s) => ({
            comments: {
                ...s.comments,
                [entryId]: change(s.comments[entryId] ?? []),
            },
        }));

    return {
        comments: {},
        status: LoadStatus.NONE,
        statusMessage: "",
        error: undefined,
        retrieveComments: (hdid) =>
            inProgress.share(hdid, async () => {
                if (get().status === LoadStatus.LOADED) {
                    logger.debug("Comments found stored, not querying!");
                    return;
                }
                logger.debug("Retrieving User comments");
                set({ status: LoadStatus.REQUESTED });
                try {
                    const result =
                        await services.userComment.getCommentsForProfile(hdid);
                    if (result.resultStatus === ResultType.Success)
                        set({
                            comments: result.resourcePayload,
                            error: undefined,
                            status: LoadStatus.LOADED,
                        });
                    else {
                        if (result.resultError)
                            throw ResultError.fromModel(result.resultError);
                        logger.warn(`Comments retrieval failed!`);
                    }
                } catch (error) {
                    rethrowIfSessionEnded(error);
                    handleError(error, ErrorType.Retrieve);
                    throw error;
                }
            }),
        createComment: async (hdid, comment) => {
            try {
                const result = await services.userComment.createComment(
                    hdid,
                    comment
                );
                if (result === undefined)
                    logger.debug("Comment creation return undefined!");
                else
                    withEntry(result.parentEntryId, (list) =>
                        [...list, result].sort(commentsSort)
                    );
                return result;
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Create);
                throw error;
            }
        },
        updateComment: async (hdid, comment) => {
            try {
                const result = await services.userComment.updateComment(
                    hdid,
                    comment
                );
                withEntry(result.parentEntryId, (list) =>
                    list.map((c) => (c.id === result.id ? result : c))
                );
                return result;
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Update);
                throw error;
            }
        },
        deleteComment: async (hdid, comment) => {
            try {
                await services.userComment.deleteComment(hdid, comment);
                withEntry(comment.parentEntryId, (list) =>
                    list.filter((c) => c.id !== comment.id)
                );
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Delete);
                throw error;
            }
        },
        reset: () => {
            inProgress.clear();
            set({
                comments: {},
                status: LoadStatus.NONE,
                statusMessage: "",
                error: undefined,
            });
        },
    };
});

export const selectCommentsAreLoading = (s: CommentState) =>
    s.status === LoadStatus.REQUESTED;
/** getEntryComments: null when the entry has none recorded */
export const entryCommentsGetter =
    (comments: Record<string, UserComment[]>) => (entryId: string) =>
        comments[entryId] === undefined ? null : comments[entryId];
