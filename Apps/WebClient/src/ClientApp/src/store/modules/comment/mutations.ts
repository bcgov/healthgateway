import Vue from "vue";
import { MutationTree } from "vuex";

import { Dictionary } from "@/models/baseTypes";
import { DateWrapper } from "@/models/dateWrapper";
import { CommentState, LoadStatus } from "@/models/storeState";
import { UserComment } from "@/models/userComment";

export const mutations: MutationTree<CommentState> = {
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

        state.profileComments[
            userComment.parentEntryId
        ] = state.profileComments[userComment.parentEntryId].sort((a, b) => {
            const firstDate = new DateWrapper(a.createdDateTime, {
                isUtc: true,
            });
            const secondDate = new DateWrapper(b.createdDateTime, {
                isUtc: true,
            });

            const value = secondDate.isAfter(firstDate)
                ? -1
                : firstDate.isAfter(secondDate)
                ? 1
                : 0;

            return value;
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
            delete state.profileComments[userComment.parentEntryId][
                commentIndex
            ];
            state.profileComments[userComment.parentEntryId].splice(
                commentIndex,
                1
            );
        }
    },

    commentError(state: CommentState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
