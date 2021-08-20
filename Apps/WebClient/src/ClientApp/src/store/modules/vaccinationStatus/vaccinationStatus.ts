import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { VaccinationStatusModule, VaccinationStatusState } from "./types";

const state: VaccinationStatusState = {
    vaccinationStatus: undefined,
    error: undefined,
    status: LoadStatus.NONE,
    statusMessage: "",
};

const namespaced = true;

export const vaccinationStatus: VaccinationStatusModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
