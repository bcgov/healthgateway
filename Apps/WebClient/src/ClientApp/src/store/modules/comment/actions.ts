import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/requestResult";
import { CommentState, LoadStatus, RootState } from "@/models/storeState";
import { UserComment } from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger, IUserCommentService } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const commentService: IUserCommentService = container.get<IUserCommentService>(
    SERVICE_IDENTIFIER.UserCommentService
);

export const actions: ActionTree<CommentState, RootState> = {
    retrieve(context, params: { hdid: string }): Promise<void> {
        return new Promise((resolve, reject) => {
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Comments found stored, not quering!`);
                resolve();
            } else {
                logger.debug(`Retrieving User comments`);
                context.commit("setRequested");
                commentService
                    .getCommentsForProfile(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", result.resultError);
                            reject(result.resultError);
                        } else {
                            context.commit(
                                "setProfileComments",
                                result.resourcePayload
                            );
                            resolve();
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleError", error);
                        reject(error);
                    });
            }
        });
    },
    createComment(
        context,
        params: { hdid: string; comment: UserComment }
    ): Promise<UserComment | undefined> {
        return new Promise((resolve, reject) => {
            commentService
                .createComment(params.hdid, params.comment)
                .then((resultComment) => {
                    context.commit("addComment", resultComment);
                    resolve(resultComment);
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    updateComment(
        context,
        params: { hdid: string; comment: UserComment }
    ): Promise<UserComment> {
        return new Promise((resolve, reject) => {
            commentService
                .updateComment(params.hdid, params.comment)
                .then((resultComment) => {
                    context.commit("updateComment", resultComment);
                    resolve(resultComment);
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    deleteComment(
        context,
        params: { hdid: string; comment: UserComment }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            commentService
                .deleteComment(params.hdid, params.comment)
                .then(() => {
                    context.commit("deleteComment", params.comment);
                    resolve();
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    handleError(context, error: ResultError) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("commentError", error);
        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Comment Error", error },
            { root: true }
        );
    },
};
