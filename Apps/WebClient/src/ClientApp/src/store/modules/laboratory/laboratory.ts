import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { LaboratoryModule, LaboratoryState } from "./types";

const state: LaboratoryState = {
    public: {
        publicCovidTestResponseResult: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },

    authenticated: {
        statusMessage: "",
        laboratoryOrders: [],
        error: undefined,
        status: LoadStatus.NONE,
    },
};

const namespaced = true;

export const laboratory: LaboratoryModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
