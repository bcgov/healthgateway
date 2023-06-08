import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { CommentModule, CommentState } from "./types";

const state: CommentState = {
    profileComments: {},
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

const namespaced = true;

export const comment: CommentModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
