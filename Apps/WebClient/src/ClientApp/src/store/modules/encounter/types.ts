import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import Encounter from "@/models/encounter";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface EncounterState {
    patientEncounters: Encounter[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface EncounterGetters
    extends GetterTree<EncounterState, RootState> {
    patientEncounters(state: EncounterState): Encounter[];
    encounterCount(state: EncounterState): number;
    isLoading(state: EncounterState): boolean;
}

type StoreContext = ActionContext<EncounterState, RootState>;
export interface EncounterActions
    extends ActionTree<EncounterState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<Encounter[]>>;
    handleError(context: StoreContext, error: ResultError): void;
}

export interface EncounterMutations extends MutationTree<EncounterState> {
    setRequested(state: EncounterState): void;
    setPatientEncounters(
        state: EncounterState,
        patientEncounters: Encounter[]
    ): void;
    encounterError(state: EncounterState, error: Error): void;
}

export interface EncounterModule extends Module<EncounterState, RootState> {
    namespaced: boolean;
    state: EncounterState;
    getters: EncounterGetters;
    actions: EncounterActions;
    mutations: EncounterMutations;
}
