import { Encounter } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterMutations, EncounterState } from "./types";

export const mutations: EncounterMutations = {
    setPatientEncountersRequested(state: EncounterState) {
        state.encounter.status = LoadStatus.REQUESTED;
    },
    setPatientEncounters(
        state: EncounterState,
        patientEncounters: Encounter[]
    ) {
        state.encounter.patientEncounters = patientEncounters;
        state.encounter.error = undefined;
        state.encounter.statusMessage = "success";
        state.encounter.status = LoadStatus.LOADED;
    },
    encounterError(state: EncounterState, error: ResultError) {
        state.encounter.error = error;
        state.encounter.statusMessage = error.resultMessage;
        state.encounter.status = LoadStatus.ERROR;
    },
    setHospitalVisitsRequested(state: EncounterState) {
        state.hospitalVisit.status = LoadStatus.REQUESTED;
    },
    setHospitalVisits(
        state: EncounterState,
        hospitalVisitResult: HospitalVisitResult
    ) {
        state.hospitalVisit.hospitalVisits = hospitalVisitResult.hospitalVisits;
        state.hospitalVisit.error = undefined;
        state.hospitalVisit.statusMessage = "success";
        state.hospitalVisit.status = LoadStatus.LOADED;
        state.hospitalVisit.queued = hospitalVisitResult.queued;
    },
    setHospitalVisitRefreshInProgress(
        state: EncounterState,
        hospitalVisitResult: HospitalVisitResult
    ) {
        state.hospitalVisit.hospitalVisits = hospitalVisitResult.hospitalVisits;
        state.hospitalVisit.error = undefined;
        state.hospitalVisit.statusMessage = "";
        state.hospitalVisit.status = LoadStatus.REQUESTED;
        state.hospitalVisit.queued = hospitalVisitResult.queued;
    },
    hospitalVisitsError(state: EncounterState, error: ResultError) {
        state.hospitalVisit.error = error;
        state.hospitalVisit.statusMessage = error.resultMessage;
        state.hospitalVisit.status = LoadStatus.ERROR;
    },
};
