import Encounter from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterMutations, EncounterState } from "./types";

export const mutations: EncounterMutations = {
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
