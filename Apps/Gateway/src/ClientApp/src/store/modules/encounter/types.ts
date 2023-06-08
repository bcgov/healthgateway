import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { HealthVisitState, HospitalVisitState } from "@/models/datasetState";
import { Encounter, HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import { RootState } from "@/store/types";

export interface EncounterState {
    healthVisits: Dictionary<HealthVisitState>;
    hospitalVisits: Dictionary<HospitalVisitState>;
}

export interface EncounterGetters
    extends GetterTree<EncounterState, RootState> {
    healthVisits(state: EncounterState): (hdid: string) => Encounter[];
    healthVisitsCount(state: EncounterState): (hdid: string) => number;
    healthVisitsAreLoading(state: EncounterState): (hdid: string) => boolean;
    hospitalVisits(state: EncounterState): (hdid: string) => HospitalVisit[];
    hospitalVisitsCount(state: EncounterState): (hdid: string) => number;
    hospitalVisitsAreLoading(state: EncounterState): (hdid: string) => boolean;
    hospitalVisitsAreQueued(state: EncounterState): (hdid: string) => boolean;
}

type StoreContext = ActionContext<EncounterState, RootState>;
export interface EncounterActions
    extends ActionTree<EncounterState, RootState> {
    retrieveHealthVisits(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<Encounter[]>>;
    retrieveHospitalVisits(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<HospitalVisitResult>>;
    handleError(
        context: StoreContext,
        params: {
            hdid: string;
            error: ResultError;
            errorType: ErrorType;
            errorSourceType: ErrorSourceType;
        }
    ): void;
}

export interface EncounterMutations extends MutationTree<EncounterState> {
    setHealthVisitsRequested(state: EncounterState, hdid: string): void;
    setHealthVisits(
        state: EncounterState,
        payload: {
            hdid: string;
            healthVisits: Encounter[];
        }
    ): void;
    setHealthVisitsError(
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
    setHospitalVisitsError(
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
