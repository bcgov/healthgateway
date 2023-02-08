import { Encounter, HospitalVisit } from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterGetters, EncounterState } from "./types";
import { getEncounterState, getHospitalVisitState } from "./util";

export const getters: EncounterGetters = {
    patientEncounters(state: EncounterState): (hdid: string) => Encounter[] {
        return (hdid: string) => getEncounterState(state, hdid).data;
    },
    encounterCount(state: EncounterState): (hdid: string) => number {
        return (hdid: string) => getEncounterState(state, hdid).data.length;
    },
    isEncounterLoading(state: EncounterState): (hdid: string) => boolean {
        return (hdid: string) =>
            getEncounterState(state, hdid).status === LoadStatus.REQUESTED;
    },
    hospitalVisits(state: EncounterState): (hdid: string) => HospitalVisit[] {
        return (hdid: string) => getHospitalVisitState(state, hdid).data;
    },
    hospitalVisitCount(state: EncounterState): (hdid: string) => number {
        return (hdid: string) => getHospitalVisitState(state, hdid).data.length;
    },
    isHospitalVisitLoading(state: EncounterState): (hdid: string) => boolean {
        return (hdid: string) =>
            getHospitalVisitState(state, hdid).status === LoadStatus.REQUESTED;
    },
    isHospitalVisitQueued(state: EncounterState): (hdid: string) => boolean {
        return (hdid: string) => getHospitalVisitState(state, hdid).queued;
    },
};
