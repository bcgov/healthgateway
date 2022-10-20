import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { EncounterModule, EncounterState } from "./types";

const state: EncounterState = {
    statusMessage: "",
    patientEncounters: [],
    hospitalVisits: [],
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const encounter: EncounterModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
