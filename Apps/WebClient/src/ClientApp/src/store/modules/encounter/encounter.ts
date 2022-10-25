import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { EncounterModule, EncounterState } from "./types";

const state: EncounterState = {
    encounter: {
        statusMessage: "",
        patientEncounters: [],
        error: undefined,
        status: LoadStatus.NONE,
    },
    hospitalVisit: {
        statusMessage: "",
        hospitalVisits: [],
        error: undefined,
        status: LoadStatus.NONE,
        queued: false,
    },
};

const namespaced = true;

export const encounter: EncounterModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
