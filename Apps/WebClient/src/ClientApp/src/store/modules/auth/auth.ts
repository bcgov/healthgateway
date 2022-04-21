import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { AuthModule, AuthState } from "./types";

export const state: AuthState = {
    statusMessage: "",
    authentication: { isChecked: false },
    error: undefined,
    isAuthenticated: false,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const auth: AuthModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
