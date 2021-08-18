import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { VaccinationStatusModule, VaccinationStatusState } from "./types";

const state: VaccinationStatusState = {
    statusMessage: "",
    vaccinationStatus: undefined,
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const vaccinationStatus: VaccinationStatusModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
