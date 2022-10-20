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
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface EncounterState {
    patientEncounters: Encounter[];
    hospitalVisits: HospitalVisit[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface EncounterGetters
    extends GetterTree<EncounterState, RootState> {
    patientEncounters(state: EncounterState): Encounter[];
    hospitalVisits(state: EncounterState): HospitalVisit[];
    encounterCount(state: EncounterState): number;
    isLoading(state: EncounterState): boolean;
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
    ): Promise<RequestResult<HospitalVisit[]>>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface EncounterMutations extends MutationTree<EncounterState> {
    setRequested(state: EncounterState): void;
    setPatientEncounters(
        state: EncounterState,
        patientEncounters: Encounter[]
    ): void;
    setHospitalVisits(
        state: EncounterState,
        hospitalVisits: HospitalVisit[]
    ): void;
    encounterError(state: EncounterState, error: ResultError): void;
}

export interface EncounterModule extends Module<EncounterState, RootState> {
    namespaced: boolean;
    state: EncounterState;
    getters: EncounterGetters;
    actions: EncounterActions;
    mutations: EncounterMutations;
}
