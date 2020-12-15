import { ActionTree, Commit } from "vuex";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { CommentState, RootState } from "@/models/storeState";
import { ILogger, IUserCommentService } from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { UserComment } from "@/models/userComment";
import { Dictionary } from "@/models/baseTypes";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`ERROR: ${JSON.stringify(error)}`);
    commit("commentError");
}

const commentService: IUserCommentService = container.get<IUserCommentService>(
    SERVICE_IDENTIFIER.UserCommentService
);

export const actions: ActionTree<CommentState, RootState> = {
    retrieveProfileComments(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<Dictionary<UserComment[]>>> {
        return new Promise((resolve, reject) => {
            const profileComments: Dictionary<
                UserComment[]
            > = context.getters.getStoredProfileComments();
            if (Object.keys(profileComments).length > 0) {
                logger.debug(`Comments found stored, not quering!`);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: profileComments,
                    resultStatus: ResultType.Success,
                    totalResultCount: Object.keys(profileComments).length,
                });
            } else {
                logger.debug(`Retrieving User comments`);
                commentService
                    .getCommentsForProfile(params.hdid)
                    .then((results) => {
                        context.commit(
                            "setProfileComments",
                            results.resourcePayload
                        );
                        resolve(results);
                    })
                    .catch((error) => {
                        handleError(context.commit, error);
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
                    handleError(context.commit, error);
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
                    handleError(context.commit, error);
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
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
};
