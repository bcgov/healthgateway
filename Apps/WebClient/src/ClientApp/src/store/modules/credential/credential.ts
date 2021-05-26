import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { CredentialModule, CredentialState } from "./types";

const state: CredentialState = {
    connection: undefined,
    credentials: [],
    statusMessage: "",
    error: false,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const credential: CredentialModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
