import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface DependentState {
    dependents: Dependent[];
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}

export interface DependentGetters
    extends GetterTree<DependentState, RootState> {
    dependents(state: DependentState): Dependent[];
    dependentsAreLoading(state: DependentState): boolean;
}

type StoreContext = ActionContext<DependentState, RootState>;
export interface DependentActions
    extends ActionTree<DependentState, RootState> {
    retrieveDependents(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<void>;
    handleDependentsError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface DependentMutations extends MutationTree<DependentState> {
    setDependentsRequested(state: DependentState): void;
    setDependents(state: DependentState, dependents: Dependent[]): void;
    setDependentsError(state: DependentState, error: ResultError): void;
}

export interface DependentModule extends Module<DependentState, RootState> {
    state: DependentState;
    getters: DependentGetters;
    actions: DependentActions;
    mutations: DependentMutations;
}
