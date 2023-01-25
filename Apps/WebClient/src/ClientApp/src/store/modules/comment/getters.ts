import { Dictionary } from "@/models/baseTypes";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";

import { CommentGetters, CommentState } from "./types";

export const getters: CommentGetters = {
    comments(state: CommentState): Dictionary<UserComment[]> {
        return state.profileComments;
    },
    getEntryComments:
        (state: CommentState) =>
        (entryId: string): UserComment[] | null => {
            if (state.profileComments[entryId] !== undefined) {
                return state.profileComments[entryId];
            } else {
                return null;
            }
        },
    entryHasComments:
        (state: CommentState) =>
        (entryId: string): boolean =>
            entryId in state.profileComments,
    isLoading(state: CommentState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
