import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { DependentModule, DependentState } from "./types";

const state: DependentState = {
    dependents: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

const namespaced = true;

export const dependent: DependentModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
