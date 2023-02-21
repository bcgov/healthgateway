import { Encounter, HospitalVisit } from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterGetters, EncounterState } from "./types";
import { getHealthVisitState, getHospitalVisitState } from "./util";

export const getters: EncounterGetters = {
    healthVisits(state: EncounterState): (hdid: string) => Encounter[] {
        return (hdid: string) => getHealthVisitState(state, hdid).data;
    },
    healthVisitsCount(state: EncounterState): (hdid: string) => number {
        return (hdid: string) => getHealthVisitState(state, hdid).data.length;
    },
    healthVisitsAreLoading(state: EncounterState): (hdid: string) => boolean {
        return (hdid: string) =>
            getHealthVisitState(state, hdid).status === LoadStatus.REQUESTED;
    },
    hospitalVisits(state: EncounterState): (hdid: string) => HospitalVisit[] {
        return (hdid: string) => getHospitalVisitState(state, hdid).data;
    },
    hospitalVisitsCount(state: EncounterState): (hdid: string) => number {
        return (hdid: string) => getHospitalVisitState(state, hdid).data.length;
    },
    hospitalVisitsAreLoading(state: EncounterState): (hdid: string) => boolean {
        return (hdid: string) =>
            getHospitalVisitState(state, hdid).status === LoadStatus.REQUESTED;
    },
    hospitalVisitsAreQueued(state: EncounterState): (hdid: string) => boolean {
        return (hdid: string) => getHospitalVisitState(state, hdid).queued;
    },
};
