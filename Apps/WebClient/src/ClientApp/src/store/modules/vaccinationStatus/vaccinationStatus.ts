import { LoadStatus } from "@/models/storeOperations";
import VaccinationRecord from "@/models/vaccinationRecord";

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
    publicVaccineRecord: {
        vaccinationRecord: undefined,
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
        activeHdid: "",
        statusChanges: 0,
        vaccinationRecords: new Map<string, VaccinationRecord>(),
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
