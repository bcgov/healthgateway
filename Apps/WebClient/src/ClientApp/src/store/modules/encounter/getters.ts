import Encounter from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterGetters, EncounterState } from "./types";

export const getters: EncounterGetters = {
    patientEncounters(state: EncounterState): Encounter[] {
        return state.patientEncounters;
    },
    encounterCount(state: EncounterState): number {
        return state.patientEncounters.length;
    },
    isLoading(state: EncounterState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
