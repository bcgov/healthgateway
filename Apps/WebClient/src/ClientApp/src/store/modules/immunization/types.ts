import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import ImmunizationResult from "@/models/immunizationResult";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface ImmunizationState {
    immunizations: ImmunizationEvent[];
    recommendations: Recommendation[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface ImmunizationGetters
    extends GetterTree<ImmunizationState, RootState> {
    immunizations(state: ImmunizationState): ImmunizationEvent[];
    covidImmunizations(state: ImmunizationState): ImmunizationEvent[];
    recomendations(state: ImmunizationState): Recommendation[];
    immunizationCount(state: ImmunizationState): number;
    isDeferredLoad(state: ImmunizationState): boolean;
    isLoading(state: ImmunizationState): boolean;
}

type StoreContext = ActionContext<ImmunizationState, RootState>;
export interface ImmunizationActions
    extends ActionTree<ImmunizationState, RootState> {
    retrieve(context: StoreContext, params: { hdid: string }): Promise<void>;
    handleError(context: StoreContext, error: ResultError): void;
}

export interface ImmunizationMutations extends MutationTree<ImmunizationState> {
    setRequested(state: ImmunizationState): void;
    setImmunizationResult(
        state: ImmunizationState,
        immunizationResult: ImmunizationResult
    ): void;
    immunizationError(state: ImmunizationState, error: Error): void;
}

export interface ImmunizationModule
    extends Module<ImmunizationState, RootState> {
    namespaced: boolean;
    state: ImmunizationState;
    getters: ImmunizationGetters;
    actions: ImmunizationActions;
    mutations: ImmunizationMutations;
}
