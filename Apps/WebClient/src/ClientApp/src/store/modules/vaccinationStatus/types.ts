import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { StringISODate } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import { RootState } from "@/store/types";

export interface VaccinationStatusState {
    vaccinationStatus: VaccinationStatus | undefined;
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface VaccinationStatusGetters
    extends GetterTree<VaccinationStatusState, RootState> {
    vaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined;
    isLoading(state: VaccinationStatusState): boolean;
}

type StoreContext = ActionContext<VaccinationStatusState, RootState>;
export interface VaccinationStatusActions
    extends ActionTree<VaccinationStatusState, RootState> {
    retrieve(
        context: StoreContext,
        params: { phn: string; dateOfBirth: StringISODate }
    ): Promise<void>;
    handleError(context: StoreContext, error: ResultError): void;
}

export interface VaccinationStatusMutations
    extends MutationTree<VaccinationStatusState> {
    setRequested(state: VaccinationStatusState): void;
    setVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ): void;
    vaccinationStatusError(state: VaccinationStatusState, error: Error): void;
}

export interface VaccinationStatusModule
    extends Module<VaccinationStatusState, RootState> {
    namespaced: boolean;
    state: VaccinationStatusState;
    getters: VaccinationStatusGetters;
    actions: VaccinationStatusActions;
    mutations: VaccinationStatusMutations;
}
