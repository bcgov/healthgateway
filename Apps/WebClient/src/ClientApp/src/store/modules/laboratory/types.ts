import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { Covid19TestResultState, LabResultState } from "@/models/datasetState";
import { StringISODate } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryOrderResult,
    LaboratoryOrder,
    LaboratoryOrderResult,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface LaboratoryState {
    publicCovid19: {
        publicCovidTestResponseResult?: PublicCovidTestResponseResult;
        statusMessage: string;
        error?: CustomBannerError;
        status?: LoadStatus;
    };
    covid19TestResults: Dictionary<Covid19TestResultState>;
    labResults: Dictionary<LabResultState>;
}

export interface LaboratoryGetters
    extends GetterTree<LaboratoryState, RootState> {
    covid19LaboratoryOrders(
        state: LaboratoryState
    ): (hdid: string) => Covid19LaboratoryOrder[];
    covid19LaboratoryOrdersCount(
        state: LaboratoryState
    ): (hdid: string) => number;
    covid19LaboratoryOrdersAreLoading(
        state: LaboratoryState
    ): (hdid: string) => boolean;
    laboratoryOrders(
        state: LaboratoryState
    ): (hdid: string) => LaboratoryOrder[];
    laboratoryOrdersCount(state: LaboratoryState): (hdid: string) => number;
    laboratoryOrdersAreLoading(
        state: LaboratoryState
    ): (hdid: string) => boolean;
    laboratoryOrdersAreQueued(
        state: LaboratoryState
    ): (hdid: string) => boolean;
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
        params: {
            hdid: string;
            error: ResultError;
            errorType: ErrorType;
            errorSourceType: ErrorSourceType;
        }
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
        params: { error: ResultError; errorType: ErrorType }
    ): void;
    resetPublicCovidTestResponseResult(context: StoreContext): void;
}

export interface LaboratoryMutations extends MutationTree<LaboratoryState> {
    setCovid19LaboratoryOrdersRequested(
        state: LaboratoryState,
        hdid: string
    ): void;
    setCovid19LaboratoryOrders(
        state: LaboratoryState,
        payload: {
            hdid: string;
            laboratoryOrders: Covid19LaboratoryOrder[];
        }
    ): void;
    setCovid19LaboratoryError(
        state: LaboratoryState,
        payload: {
            hdid: string;
            error: Error;
        }
    ): void;
    setLaboratoryOrdersRequested(state: LaboratoryState, hdid: string): void;
    setLaboratoryOrders(
        state: LaboratoryState,
        payload: {
            hdid: string;
            laboratoryOrderResult: LaboratoryOrderResult;
        }
    ): void;
    setLaboratoryOrdersRefreshInProgress(
        state: LaboratoryState,
        payload: {
            hdid: string;
            laboratoryOrderResult: LaboratoryOrderResult;
        }
    ): void;
    setLaboratoryError(
        state: LaboratoryState,
        payload: {
            hdid: string;
            error: Error;
        }
    ): void;
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
