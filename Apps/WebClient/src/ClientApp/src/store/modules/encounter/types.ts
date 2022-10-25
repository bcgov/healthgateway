import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Encounter, HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface EncounterState {
    encounter: {
        patientEncounters: Encounter[];
        statusMessage: string;
        error?: ResultError;
        status: LoadStatus;
    };
    hospitalVisit: {
        hospitalVisits: HospitalVisit[];
        statusMessage: string;
        error?: ResultError;
        status: LoadStatus;
        queued: boolean;
    };
}

export interface EncounterGetters
    extends GetterTree<EncounterState, RootState> {
    patientEncounters(state: EncounterState): Encounter[];
    encounterCount(state: EncounterState): number;
    isEncounterLoading(state: EncounterState): boolean;
    hospitalVisits(state: EncounterState): HospitalVisit[];
    hospitalVisitCount(state: EncounterState): number;
    isHospitalVisitLoading(state: EncounterState): boolean;
    isHospitalVisitQueued(state: EncounterState): boolean;
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
    setPatientEncountersRequested(state: EncounterState): void;
    setPatientEncounters(
        state: EncounterState,
        patientEncounters: Encounter[]
    ): void;
    encounterError(state: EncounterState, error: ResultError): void;
    setHospitalVisitsRequested(state: EncounterState): void;
    setHospitalVisits(
        state: EncounterState,
        hospitalVisits: HospitalVisitResult
    ): void;
    setHospitalVisitRefreshInProgress(
        state: EncounterState,
        hospitalVisits: HospitalVisitResult
    ): void;
    hospitalVisitsError(state: EncounterState, error: ResultError): void;
}

export interface EncounterModule extends Module<EncounterState, RootState> {
    namespaced: boolean;
    state: EncounterState;
    getters: EncounterGetters;
    actions: EncounterActions;
    mutations: EncounterMutations;
}
