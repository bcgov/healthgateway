import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import MedicationRequest from "@/models/MedicationRequest";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface MedicationRequestState {
    medicationRequests: MedicationRequest[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface MedicationRequestGetters
    extends GetterTree<MedicationRequestState, RootState> {
    medicationRequests(state: MedicationRequestState): MedicationRequest[];
    medicationRequestCount(state: MedicationRequestState): number;
    isMedicationRequestLoading(state: MedicationRequestState): boolean;
}

type MedicationRequestContext = ActionContext<
    MedicationRequestState,
    RootState
>;
export interface MedicationRequestActions
    extends ActionTree<MedicationRequestState, RootState> {
    retrieveMedicationRequests(
        context: MedicationRequestContext,
        params: { hdid: string }
    ): Promise<RequestResult<MedicationRequest[]>>;
    handleRequestError(
        context: MedicationRequestContext,
        error: ResultError
    ): void;
}

export interface MedicationRequestMutations
    extends MutationTree<MedicationRequestState> {
    setMedicationRequestRequested(state: MedicationRequestState): void;
    setMedicationRequestResult(
        state: MedicationRequestState,
        medicationRequestResult: RequestResult<MedicationRequest[]>
    ): void;
    medicationRequestError(state: MedicationRequestState, error: Error): void;
}

export interface MedicationRequestModule
    extends Module<MedicationRequestState, RootState> {
    getters: MedicationRequestGetters;
    actions: MedicationRequestActions;
    mutations: MedicationRequestMutations;
}
