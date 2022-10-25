import { Encounter, HospitalVisit } from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterGetters, EncounterState } from "./types";

export const getters: EncounterGetters = {
    patientEncounters(state: EncounterState): Encounter[] {
        return state.encounter.patientEncounters;
    },
    encounterCount(state: EncounterState): number {
        return state.encounter.patientEncounters.length;
    },
    isEncounterLoading(state: EncounterState): boolean {
        return state.encounter.status === LoadStatus.REQUESTED;
    },
    hospitalVisits(state: EncounterState): HospitalVisit[] {
        return state.hospitalVisit.hospitalVisits;
    },
    hospitalVisitCount(state: EncounterState): number {
        return state.hospitalVisit.hospitalVisits.length;
    },
    isHospitalVisitLoading(state: EncounterState): boolean {
        return state.hospitalVisit.status === LoadStatus.REQUESTED;
    },
    isHospitalVisitQueued(state: EncounterState): boolean {
        return state.hospitalVisit.queued;
    },
};
