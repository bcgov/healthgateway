import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { DatasetState, HospitalVisitState } from "@/models/datasetState";
import { Encounter, HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import { RootState } from "@/store/types";

export interface EncounterState {
    encounters: Dictionary<DatasetState<Encounter[]>>;
    hospitalVisits: Dictionary<HospitalVisitState>;
}

export interface EncounterGetters
    extends GetterTree<EncounterState, RootState> {
    patientEncounters(state: EncounterState): (hdid: string) => Encounter[];
    encounterCount(state: EncounterState): (hdid: string) => number;
    isEncounterLoading(state: EncounterState): (hdid: string) => boolean;
    hospitalVisits(state: EncounterState): (hdid: string) => HospitalVisit[];
    hospitalVisitCount(state: EncounterState): (hdid: string) => number;
    isHospitalVisitLoading(state: EncounterState): (hdid: string) => boolean;
    isHospitalVisitQueued(state: EncounterState): (hdid: string) => boolean;
}

type StoreContext = ActionContext<EncounterState, RootState>;
export interface EncounterActions
    extends ActionTree<EncounterState, RootState> {
    retrievePatientEncounters(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<Encounter[]>>;
    retrieveHospitalVisits(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<HospitalVisitResult>>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface EncounterMutations extends MutationTree<EncounterState> {
    setPatientEncountersRequested(state: EncounterState, hdid: string): void;
    setPatientEncounters(
        state: EncounterState,
        payload: {
            hdid: string;
            patientEncounters: Encounter[];
        }
    ): void;
    encounterError(
        state: EncounterState,
        payload: {
            hdid: string;
            error: ResultError;
        }
    ): void;
    setHospitalVisitsRequested(state: EncounterState, hdid: string): void;
    setHospitalVisits(
        state: EncounterState,
        payload: {
            hdid: string;
            hospitalVisitResult: HospitalVisitResult;
        }
    ): void;
    setHospitalVisitRefreshInProgress(
        state: EncounterState,
        payload: {
            hdid: string;
            hospitalVisits: HospitalVisitResult;
        }
    ): void;
    hospitalVisitsError(
        state: EncounterState,
        payload: {
            hdid: string;
            error: ResultError;
        }
    ): void;
}

export interface EncounterModule extends Module<EncounterState, RootState> {
    namespaced: boolean;
    state: EncounterState;
    getters: EncounterGetters;
    actions: EncounterActions;
    mutations: EncounterMutations;
}
