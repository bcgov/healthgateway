import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { ImmunizationDatasetState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import ImmunizationResult from "@/models/immunizationResult";
import { RootState } from "@/store/types";

export interface ImmunizationState {
    immunizations: Dictionary<ImmunizationDatasetState>;
}

export interface ImmunizationGetters
    extends GetterTree<ImmunizationState, RootState> {
    immunizations(
        state: ImmunizationState
    ): (hdid: string) => ImmunizationEvent[];
    covidImmunizations(
        state: ImmunizationState
    ): (hdid: string) => ImmunizationEvent[];
    recomendations(
        state: ImmunizationState
    ): (hdid: string) => Recommendation[];
    error(state: ImmunizationState): (hdid: string) => ResultError | undefined;
    immunizationCount(state: ImmunizationState): (hdid: string) => number;
    isDeferredLoad(state: ImmunizationState): (hdid: string) => boolean;
    isLoading(state: ImmunizationState): (hdid: string) => boolean;
}

type StoreContext = ActionContext<ImmunizationState, RootState>;
export interface ImmunizationActions
    extends ActionTree<ImmunizationState, RootState> {
    retrieve(context: StoreContext, params: { hdid: string }): Promise<void>;
    handleError(
        context: StoreContext,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ): void;
}

export interface ImmunizationMutations extends MutationTree<ImmunizationState> {
    setImmunizationRequested(state: ImmunizationState, hdid: string): void;
    setImmunizationResult(
        state: ImmunizationState,
        payload: {
            hdid: string;
            immunizationResult: ImmunizationResult;
        }
    ): void;
    immunizationError(
        state: ImmunizationState,
        payload: {
            hdid: string;
            error: ResultError;
        }
    ): void;
}

export interface ImmunizationModule
    extends Module<ImmunizationState, RootState> {
    namespaced: boolean;
    state: ImmunizationState;
    getters: ImmunizationGetters;
    actions: ImmunizationActions;
    mutations: ImmunizationMutations;
}
