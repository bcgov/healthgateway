import { MutationTree } from "vuex";

import Encounter from "@/models/encounter";
import { EncounterState, LoadStatus } from "@/models/storeState";

export const mutations: MutationTree<EncounterState> = {
    setRequested(state: EncounterState) {
        state.status = LoadStatus.REQUESTED;
    },
    setPatientEncounters(
        state: EncounterState,
        patientEncounters: Encounter[]
    ) {
        state.patientEncounters = patientEncounters;
        state.error = undefined;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    encounterError(state: EncounterState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
