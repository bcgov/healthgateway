import { GetterTree } from "vuex";

import Encounter from "@/models/encounter";
import { EncounterState, LoadStatus, RootState } from "@/models/storeState";

export const getters: GetterTree<EncounterState, RootState> = {
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
