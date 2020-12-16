import Vue from "vue";
import { MutationTree } from "vuex";
import { CommentState, StateType } from "@/models/storeState";
import { UserComment } from "@/models/userComment";
import { Dictionary } from "@/models/baseTypes";
import { DateWrapper } from "@/models/dateWrapper";

export const mutations: MutationTree<CommentState> = {
    setProfileComments(
        state: CommentState,
        profileComments: Dictionary<UserComment[]>
    ) {
        state.profileComments = profileComments;
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    addComment(state: CommentState, userComment: UserComment) {
        if (state.profileComments[userComment.parentEntryId] !== undefined) {
            state.profileComments[userComment.parentEntryId].push(userComment);
        } else {
            console.log("HERE!");
            //state.profileComments[userComment.parentEntryId] = [];
            Vue.set(state.profileComments, userComment.parentEntryId, []);
            state.profileComments[userComment.parentEntryId].push(userComment);
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

            const vale = secondDate.isAfter(firstDate)
                ? -1
                : firstDate.isAfter(secondDate)
                ? 1
                : 0;

            return vale;
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

    setEntryComments(state: CommentState, entryComments: UserComment[]) {
        state.profileComments[entryComments[0].parentEntryId] = entryComments;
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },

    commentError(state: CommentState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
