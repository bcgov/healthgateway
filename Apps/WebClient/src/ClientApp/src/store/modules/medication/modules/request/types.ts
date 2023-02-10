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
    specialAuthorityRequests(
        state: MedicationRequestState
    ): (hdid: string) => MedicationRequest[];
    specialAuthorityRequestsCount(
        state: MedicationRequestState
    ): (hdid: string) => number;
    specialAuthorityRequestsAreLoading(
        state: MedicationRequestState
    ): (hdid: string) => boolean;
}

type StoreContext = ActionContext<MedicationRequestState, RootState>;
export interface MedicationRequestActions
    extends ActionTree<MedicationRequestState, RootState> {
    retrieveSpecialAuthorityRequests(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<MedicationRequest[]>>;
    handleSpecialAuthorityRequestsError(
        context: StoreContext,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ): void;
}

export interface MedicationRequestMutations
    extends MutationTree<MedicationRequestState> {
    setSpecialAuthorityRequestsRequested(
        state: MedicationRequestState,
        hdid: string
    ): void;
    setSpecialAuthorityRequests(
        state: MedicationRequestState,
        payload: {
            hdid: string;
            specialAuthorityRequestsResult: RequestResult<MedicationRequest[]>;
        }
    ): void;
    setSpecialAuthorityRequestsError(
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
