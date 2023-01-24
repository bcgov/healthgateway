import { Dictionary } from "@/models/baseTypes";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { CommentModule, CommentState } from "./types";

const state: CommentState = {
    statusMessage: "",
    profileComments: {},
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const comment: CommentModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};

export const isDictionary = (
    dictionary: Dictionary<UserComment[]>
): dictionary is Dictionary<UserComment[]> => {
    return "comments" in dictionary;
};
