import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import BannerError from "@/models/bannerError";
import { StringISODate } from "@/models/dateWrapper";
import {
    LaboratoryOrder,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface LaboratoryState {
    public: {
        publicCovidTestResponseResult?: PublicCovidTestResponseResult;
        statusMessage: string;
        error?: BannerError;
        status?: LoadStatus;
    };
    authenticated: {
        laboratoryOrders: LaboratoryOrder[];
        statusMessage: string;
        error?: ResultError;
        status: LoadStatus;
    };
}

export interface LaboratoryGetters
    extends GetterTree<LaboratoryState, RootState> {
    laboratoryOrders(state: LaboratoryState): LaboratoryOrder[];
    laboratoryCount(state: LaboratoryState): number;
    isLoading(state: LaboratoryState): boolean;
    publicCovidTestResponseResult(
        state: LaboratoryState
    ): PublicCovidTestResponseResult | undefined;
    isPublicCovidTestResponseResultLoading(state: LaboratoryState): boolean;
    publicCovidTestResponseResultError(
        state: LaboratoryState
    ): BannerError | undefined;
    publicCovidTestResponseResultStatusMessage(state: LaboratoryState): string;
}

type StoreContext = ActionContext<LaboratoryState, RootState>;
export interface LaboratoryActions
    extends ActionTree<LaboratoryState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrder[]>>;
    handleError(context: StoreContext, error: ResultError): void;
    retrievePublicCovidTests(
        context: StoreContext,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            collectionDate: StringISODate;
        }
    ): Promise<PublicCovidTestResponseResult>;
    handlePublicCovidTestsError(
        context: StoreContext,
        error: ResultError
    ): void;
    resetPublicCovidTestResponseResult(context: StoreContext): void;
}

export interface LaboratoryMutations extends MutationTree<LaboratoryState> {
    setRequested(state: LaboratoryState): void;
    setLaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrders: LaboratoryOrder[]
    ): void;
    laboratoryError(state: LaboratoryState, error: Error): void;
    setPublicCovidTestResponseResultRequested(state: LaboratoryState): void;
    setPublicCovidTestResponseResult(
        state: LaboratoryState,
        publicCovidTestResponseResult: PublicCovidTestResponseResult
    ): void;
    setPublicCovidTestResponseResultError(
        state: LaboratoryState,
        error: BannerError
    ): void;
    setPublicCovidTestResponseResultStatusMessage(
        state: LaboratoryState,
        statusMessage: string
    ): void;
    resetPublicCovidTestResponseResult(state: LaboratoryState): void;
}

export interface LaboratoryModule extends Module<LaboratoryState, RootState> {
    state: LaboratoryState;
    getters: LaboratoryGetters;
    actions: LaboratoryActions;
    mutations: LaboratoryMutations;
}
