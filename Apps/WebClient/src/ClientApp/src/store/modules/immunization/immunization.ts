import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { ImmunizationModule, ImmunizationState } from "./types";

const state: ImmunizationState = {
    statusMessage: "",
    immunizations: [],
    recommendations: [],
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const immunization: ImmunizationModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
