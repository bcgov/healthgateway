import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { CustomBannerError } from "@/models/bannerError";
import { StringISODate } from "@/models/dateWrapper";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryOrderResult,
    LaboratoryOrder,
    LaboratoryOrderResult,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface LaboratoryState {
    publicCovid19: {
        publicCovidTestResponseResult?: PublicCovidTestResponseResult;
        statusMessage: string;
        error?: CustomBannerError;
        status?: LoadStatus;
    };
    authenticatedCovid19: {
        laboratoryOrders: Covid19LaboratoryOrder[];
        statusMessage: string;
        error?: ResultError;
        status: LoadStatus;
    };
    authenticated: {
        laboratoryOrders: LaboratoryOrder[];
        statusMessage: string;
        error?: ResultError;
        status: LoadStatus;
        queued: boolean;
    };
}

export interface LaboratoryGetters
    extends GetterTree<LaboratoryState, RootState> {
    covid19LaboratoryOrders(state: LaboratoryState): Covid19LaboratoryOrder[];
    covid19LaboratoryOrdersCount(state: LaboratoryState): number;
    covid19LaboratoryOrdersAreLoading(state: LaboratoryState): boolean;
    laboratoryOrders(state: LaboratoryState): LaboratoryOrder[];
    laboratoryOrdersCount(state: LaboratoryState): number;
    laboratoryOrdersAreLoading(state: LaboratoryState): boolean;
    laboratoryOrdersAreQueued(state: LaboratoryState): boolean;
    publicCovidTestResponseResult(
        state: LaboratoryState
    ): PublicCovidTestResponseResult | undefined;
    isPublicCovidTestResponseResultLoading(state: LaboratoryState): boolean;
    publicCovidTestResponseResultError(
        state: LaboratoryState
    ): CustomBannerError | undefined;
    publicCovidTestResponseResultStatusMessage(state: LaboratoryState): string;
}

type StoreContext = ActionContext<LaboratoryState, RootState>;
export interface LaboratoryActions
    extends ActionTree<LaboratoryState, RootState> {
    retrieveCovid19LaboratoryOrders(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>>;
    retrieveLaboratoryOrders(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrderResult>>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
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
    setCovid19LaboratoryOrdersRequested(state: LaboratoryState): void;
    setCovid19LaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrders: Covid19LaboratoryOrder[]
    ): void;
    covid19LaboratoryError(state: LaboratoryState, error: Error): void;
    setLaboratoryOrdersRequested(state: LaboratoryState): void;
    setLaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrderResult: LaboratoryOrderResult
    ): void;
    laboratoryError(state: LaboratoryState, error: Error): void;
    setPublicCovidTestResponseResultRequested(state: LaboratoryState): void;
    setPublicCovidTestResponseResult(
        state: LaboratoryState,
        publicCovidTestResponseResult: PublicCovidTestResponseResult
    ): void;
    setPublicCovidTestResponseResultError(
        state: LaboratoryState,
        error: CustomBannerError
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
