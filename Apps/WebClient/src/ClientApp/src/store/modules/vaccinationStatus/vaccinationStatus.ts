import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { VaccinationStatusModule, VaccinationStatusState } from "./types";

const state: VaccinationStatusState = {
    public: {
        vaccinationStatus: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    authenticated: {
        vaccinationStatus: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    authenticatedVaccineRecord: {
        vaccinationRecord: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
};

const namespaced = true;

export const vaccinationStatus: VaccinationStatusModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
