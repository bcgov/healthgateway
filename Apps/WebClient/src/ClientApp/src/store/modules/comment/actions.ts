import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IUserCommentService } from "@/services/interfaces";

import { CommentActions } from "./types";

export const actions: CommentActions = {
    retrieve(context, params: { hdid: string }): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        const commentService: IUserCommentService =
            container.get<IUserCommentService>(
                SERVICE_IDENTIFIER.UserCommentService
            );
        return new Promise((resolve, reject) => {
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Comments found stored, not querying!`);
                resolve();
            } else {
                logger.debug(`Retrieving User comments`);
                context.commit("setRequested");
                commentService
                    .getCommentsForProfile(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", {
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        } else {
                            context.commit(
                                "setProfileComments",
                                result.resourcePayload
                            );
                            resolve();
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
    createComment(
        context,
        params: { hdid: string; comment: UserComment }
    ): Promise<UserComment | undefined> {
        const commentService: IUserCommentService =
            container.get<IUserCommentService>(
                SERVICE_IDENTIFIER.UserCommentService
            );
        return new Promise((resolve, reject) => {
            commentService
                .createComment(params.hdid, params.comment)
                .then((resultComment) => {
                    context.commit("addComment", resultComment);
                    resolve(resultComment);
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Create,
                    });
                    reject(error);
                });
        });
    },
    updateComment(
        context,
        params: { hdid: string; comment: UserComment }
    ): Promise<UserComment> {
        const commentService: IUserCommentService =
            container.get<IUserCommentService>(
                SERVICE_IDENTIFIER.UserCommentService
            );
        return new Promise((resolve, reject) => {
            commentService
                .updateComment(params.hdid, params.comment)
                .then((resultComment) => {
                    context.commit("updateComment", resultComment);
                    resolve(resultComment);
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Update,
                    });
                    reject(error);
                });
        });
    },
    deleteComment(
        context,
        params: { hdid: string; comment: UserComment }
    ): Promise<void> {
        const commentService: IUserCommentService =
            container.get<IUserCommentService>(
                SERVICE_IDENTIFIER.UserCommentService
            );
        return new Promise((resolve, reject) => {
            commentService
                .deleteComment(params.hdid, params.comment)
                .then(() => {
                    context.commit("deleteComment", params.comment);
                    resolve();
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Delete,
                    });
                    reject(error);
                });
        });
    },
    handleError(context, params: { error: ResultError; errorType: ErrorType }) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("commentError", params.error);
        context.dispatch(
            "errorBanner/addError",
            {
                errorType: params.errorType,
                source: ErrorSourceType.Comment,
                traceId: params.error.traceId,
            },
            { root: true }
        );
    },
};
