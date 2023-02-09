import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { SpecialAuthorityRequestState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import MedicationRequest from "@/models/medicationRequest";
import RequestResult from "@/models/requestResult";
import { RootState } from "@/store/types";

export interface MedicationRequestState {
    specialAuthorityRequests: Dictionary<SpecialAuthorityRequestState>;
}

export interface MedicationRequestGetters
    extends GetterTree<MedicationRequestState, RootState> {
    medicationRequests(
        state: MedicationRequestState
    ): (hdid: string) => MedicationRequest[];
    medicationRequestCount(
        state: MedicationRequestState
    ): (hdid: string) => number;
    isMedicationRequestLoading(
        state: MedicationRequestState
    ): (hdid: string) => boolean;
}

type StoreContext = ActionContext<MedicationRequestState, RootState>;
export interface MedicationRequestActions
    extends ActionTree<MedicationRequestState, RootState> {
    retrieveMedicationRequests(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<MedicationRequest[]>>;
    handleMedicationRequestError(
        context: StoreContext,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ): void;
}

export interface MedicationRequestMutations
    extends MutationTree<MedicationRequestState> {
    setMedicationRequestRequested(
        state: MedicationRequestState,
        hdid: string
    ): void;
    setMedicationRequestResult(
        state: MedicationRequestState,
        payload: {
            hdid: string;
            medicationRequestResult: RequestResult<MedicationRequest[]>;
        }
    ): void;
    medicationRequestError(
        state: MedicationRequestState,
        payload: {
            hdid: string;
            error: Error;
        }
    ): void;
}

export interface MedicationRequestModule
    extends Module<MedicationRequestState, RootState> {
    getters: MedicationRequestGetters;
    actions: MedicationRequestActions;
    mutations: MedicationRequestMutations;
}
