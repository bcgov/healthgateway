import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { Dictionary } from "@/models/baseTypes";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";
import { ILogger, IUserCommentService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import DateSortUtility from "@/utility/dateSortUtility";

const commentsSort = (a: UserComment, b: UserComment): number =>
    DateSortUtility.ascending(
        DateWrapper.fromIso(a.createdDateTime),
        DateWrapper.fromIso(b.createdDateTime)
    );

export const useCommentStore = defineStore("comment", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const commentService = container.get<IUserCommentService>(
        SERVICE_IDENTIFIER.UserCommentService
    );

    const errorStore = useErrorStore();

    const comments = ref<Dictionary<UserComment[]>>({});
    const status = ref(LoadStatus.NONE);
    const statusMessage = ref("");
    const error = ref<ResultError>();

    const commentsAreLoading = computed(
        () => status.value === LoadStatus.REQUESTED
    );

    function getEntryComments(entryId: string): UserComment[] | null {
        if (comments.value[entryId] !== undefined) {
            return comments.value[entryId];
        } else {
            return null;
        }
    }

    function entryHasComments(entryId: string): boolean {
        return comments.value[entryId] !== undefined;
    }

    function setCommentsRequested() {
        status.value = LoadStatus.REQUESTED;
    }

    function setComments(incomingProfileComments: Dictionary<UserComment[]>) {
        comments.value = incomingProfileComments;
        error.value = undefined;
        status.value = LoadStatus.LOADED;
    }

    function addComment(userComment: UserComment) {
        if (comments.value[userComment.parentEntryId] !== undefined) {
            comments.value[userComment.parentEntryId].push(userComment);
        } else {
            comments.value[userComment.parentEntryId] = [userComment];
        }

        comments.value[userComment.parentEntryId].sort(commentsSort);
    }

    function replaceComment(userComment: UserComment) {
        const commentIndex = comments.value[
            userComment.parentEntryId
        ].findIndex((x) => x.id === userComment.id);
        comments.value[userComment.parentEntryId][commentIndex] = userComment;
    }

    function removeComment(userComment: UserComment) {
        const commentIndex = comments.value[
            userComment.parentEntryId
        ].findIndex((x) => x.id === userComment.id);
        comments.value[userComment.parentEntryId].splice(commentIndex, 1);
    }

    function setCommentsError(errorRaised: ResultError) {
        error.value = errorRaised;
        status.value = LoadStatus.ERROR;
        statusMessage.value = errorRaised.message;
    }

    function handleError(errorRaised: ResultError, errorType: ErrorType) {
        logger.error(`ERROR: ${JSON.stringify(errorRaised)}`);
        setCommentsError(errorRaised);

        if (errorRaised.statusCode === 429) {
            const errorKey = "page";
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning(errorKey);
            } else {
                errorStore.setTooManyRequestsError(errorKey);
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Comment,
                errorRaised.traceId
            );
        }
    }

    function retrieveComments(hdid: string): Promise<void> {
        if (status.value === LoadStatus.LOADED) {
            logger.debug(`Comments found stored, not querying!`);
            return Promise.resolve();
        } else {
            logger.debug(`Retrieving User comments`);
            setCommentsRequested();
            return commentService
                .getCommentsForProfile(hdid)
                .then((result) => {
                    if (result.resultStatus === ResultType.Success) {
                        setComments(result.resourcePayload);
                    } else {
                        if (result.resultError) {
                            throw ResultError.fromResultErrorDetails(
                                result.resultError
                            );
                        }
                        logger.warn(
                            `Comments retrieval failed! ${JSON.stringify(
                                result
                            )}`
                        );
                    }
                })
                .catch((error: ResultError) => {
                    handleError(error, ErrorType.Retrieve);
                    throw error;
                });
        }
    }

    function createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment | undefined> {
        return commentService
            .createComment(hdid, comment)
            .then((result) => {
                if (result !== undefined) {
                    addComment(result);
                } else {
                    logger.debug(`Comment creation return undefined!`);
                }
                return result;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Create);
                throw error;
            });
    }

    function updateComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment> {
        return commentService
            .updateComment(hdid, comment)
            .then((result) => {
                replaceComment(result);
                return result;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Update);
                throw error;
            });
    }

    function deleteComment(hdid: string, comment: UserComment): Promise<void> {
        return commentService
            .deleteComment(hdid, comment)
            .then(() => {
                removeComment(comment);
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Delete);
                throw error;
            });
    }

    return {
        comments,
        commentsAreLoading,
        getEntryComments,
        entryHasComments,
        retrieveComments,
        createComment,
        updateComment,
        deleteComment,
    };
});
