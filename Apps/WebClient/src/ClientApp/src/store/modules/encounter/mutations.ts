import { DatasetState, HospitalVisitState } from "@/models/datasetState";
import { Encounter } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterMutations, EncounterState } from "./types";
import {
    getHealthVisitState,
    getHospitalVisitState,
    setHealthVisitState,
    setHospitalVisitState,
} from "./util";

export const mutations: EncounterMutations = {
    setHealthVisitsRequested(state: EncounterState, hdid: string) {
        const currentState = getHealthVisitState(state, hdid);
        const nextState: DatasetState<Encounter[]> = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setHealthVisitState(state, hdid, nextState);
    },
    setHealthVisits(
        state: EncounterState,
        payload: { hdid: string; healthVisits: Encounter[] }
    ) {
        const { hdid, healthVisits } = payload;
        const currentState = getHealthVisitState(state, hdid);
        const nextState: DatasetState<Encounter[]> = {
            ...currentState,
            data: healthVisits,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
        };
        setHealthVisitState(state, hdid, nextState);
    },
    setHealthVisitsError(
        state: EncounterState,
        payload: { hdid: string; error: ResultError }
    ) {
        const { hdid, error } = payload;
        const currentState = getHealthVisitState(state, hdid);
        const nextState: DatasetState<Encounter[]> = {
            ...currentState,
            error: error,
            statusMessage: error.resultMessage,
            status: LoadStatus.ERROR,
        };
        setHealthVisitState(state, hdid, nextState);
    },
    setHospitalVisitsRequested(state: EncounterState, hdid: string) {
        const currentState = getHospitalVisitState(state, hdid);
        const nextState: HospitalVisitState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setHospitalVisitState(state, hdid, nextState);
    },
    setHospitalVisits(
        state: EncounterState,
        payload: { hdid: string; hospitalVisitResult: HospitalVisitResult }
    ) {
        const { hdid, hospitalVisitResult } = payload;
        const currentState = getHospitalVisitState(state, hdid);
        const nextState: HospitalVisitState = {
            ...currentState,
            data: hospitalVisitResult.hospitalVisits,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
            queued: hospitalVisitResult.queued,
        };
        setHospitalVisitState(state, hdid, nextState);
    },
    setHospitalVisitRefreshInProgress(
        state: EncounterState,
        payload: { hdid: string; hospitalVisits: HospitalVisitResult }
    ) {
        const { hdid, hospitalVisits } = payload;
        const currentState = getHospitalVisitState(state, hdid);
        const nextState: HospitalVisitState = {
            ...currentState,
            data: hospitalVisits.hospitalVisits,
            error: undefined,
            statusMessage: "",
            status: LoadStatus.REQUESTED,
            queued: hospitalVisits.queued,
        };
        setHospitalVisitState(state, hdid, nextState);
    },
    setHospitalVisitsError(
        state: EncounterState,
        payload: { hdid: string; error: ResultError }
    ) {
        const { hdid, error } = payload;
        const currentState = getHospitalVisitState(state, hdid);
        const nextState: HospitalVisitState = {
            ...currentState,
            error: error,
            statusMessage: error.resultMessage,
            status: LoadStatus.ERROR,
        };
        setHospitalVisitState(state, hdid, nextState);
    },
};
