import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { LaboratoryOrder } from "@/models/laboratory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface LaboratoryState {
    laboratoryOrders: LaboratoryOrder[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface LaboratoryGetters
    extends GetterTree<LaboratoryState, RootState> {
    laboratoryOrders(state: LaboratoryState): LaboratoryOrder[];
    laboratoryCount(state: LaboratoryState): number;
    isLoading(state: LaboratoryState): boolean;
}

type StoreContext = ActionContext<LaboratoryState, RootState>;
export interface LaboratoryActions
    extends ActionTree<LaboratoryState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrder[]>>;
    handleError(context: StoreContext, error: ResultError): void;
}

export interface LaboratoryMutations extends MutationTree<LaboratoryState> {
    setRequested(state: LaboratoryState): void;
    setLaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrders: LaboratoryOrder[]
    ): void;
    laboratoryError(state: LaboratoryState, error: Error): void;
}

export interface LaboratoryModule extends Module<LaboratoryState, RootState> {
    state: LaboratoryState;
    getters: LaboratoryGetters;
    actions: LaboratoryActions;
    mutations: LaboratoryMutations;
}
