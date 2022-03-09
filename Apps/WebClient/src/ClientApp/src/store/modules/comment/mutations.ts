import Vue from "vue";

import { Dictionary } from "@/models/baseTypes";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";

import { CommentMutations, CommentState } from "./types";

export const mutations: CommentMutations = {
    setRequested(state: CommentState) {
        state.status = LoadStatus.REQUESTED;
    },
    setProfileComments(
        state: CommentState,
        profileComments: Dictionary<UserComment[]>
    ) {
        state.profileComments = profileComments;
        state.error = undefined;
        state.status = LoadStatus.LOADED;
    },
    addComment(state: CommentState, userComment: UserComment) {
        if (state.profileComments[userComment.parentEntryId] !== undefined) {
            state.profileComments[userComment.parentEntryId].push(userComment);
        } else {
            Vue.set(state.profileComments, userComment.parentEntryId, [
                userComment,
            ]);
        }

        state.profileComments[userComment.parentEntryId].sort((a, b) => {
            const firstDate = new DateWrapper(a.createdDateTime, {
                isUtc: true,
            });
            const secondDate = new DateWrapper(b.createdDateTime, {
                isUtc: true,
            });

            if (firstDate.isAfter(secondDate)) {
                return 1;
            }
            if (firstDate.isBefore(secondDate)) {
                return -1;
            }
            return 0;
        });
    },
    updateComment(state: CommentState, userComment: UserComment) {
        const commentIndex = state.profileComments[
            userComment.parentEntryId
        ].findIndex((x) => x.id === userComment.id);

        Vue.set(
            state.profileComments[userComment.parentEntryId],
            commentIndex,
            userComment
        );
    },

    deleteComment(state: CommentState, userComment: UserComment) {
        const commentIndex = state.profileComments[
            userComment.parentEntryId
        ].findIndex((x) => x.id === userComment.id);

        if (commentIndex > -1) {
            state.profileComments[userComment.parentEntryId].splice(
                commentIndex,
                1
            );
        }
    },

    commentError(state: CommentState, error: ResultError) {
        state.error = error;
        state.statusMessage = error.resultMessage;
        state.status = LoadStatus.ERROR;
    },
};
