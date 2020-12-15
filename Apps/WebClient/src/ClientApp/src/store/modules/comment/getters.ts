import { GetterTree } from "vuex";
import { CommentState, RootState } from "@/models/storeState";
import { Dictionary } from "@/models/baseTypes";
import { UserComment } from "@/models/userComment";

export const getters: GetterTree<CommentState, RootState> = {
    getStoredProfileComments: (state: CommentState) => (): Dictionary<
        UserComment[]
    > => {
        return state.profileComments;
    },
    getEntryComments: (state: CommentState) => (
        entryId: string
    ): UserComment[] | undefined => {
        return state.profileComments[entryId];
    },
    entryHasComments: (state: CommentState) => (entryId: string): boolean => {
        return entryId in state.profileComments;
    },
};
