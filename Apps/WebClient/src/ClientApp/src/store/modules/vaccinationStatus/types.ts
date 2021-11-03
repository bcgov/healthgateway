import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import { RootState } from "@/store/types";

export interface VaccinationStatusState {
    public: {
        vaccinationStatus?: VaccinationStatus;
        error?: BannerError;
        status: LoadStatus;
        statusMessage: string;
    };
    publicVaccineRecord: {
        vaccinationRecord?: CovidVaccineRecord;
        status: LoadStatus;
        statusMessage: string;
    };
    authenticated: {
        vaccinationStatus?: VaccinationStatus;
        error?: BannerError;
        status: LoadStatus;
        statusMessage: string;
    };
    authenticatedVaccineRecord: {
        vaccinationRecord?: CovidVaccineRecord;
        error?: BannerError;
        status: LoadStatus;
        statusMessage: string;
        resultMessage: string;
    };
}

export interface VaccinationStatusGetters
    extends GetterTree<VaccinationStatusState, RootState> {
    vaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined;
    isLoading(state: VaccinationStatusState): boolean;
    error(state: VaccinationStatusState): BannerError | undefined;
    statusMessage(state: VaccinationStatusState): string;
    publicVaccineRecord(
        state: VaccinationStatusState
    ): CovidVaccineRecord | undefined;
    publicVaccineRecordIsLoading(state: VaccinationStatusState): boolean;
    publicVaccineRecordStatusMessage(state: VaccinationStatusState): string;
    authenticatedVaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined;
    authenticatedIsLoading(state: VaccinationStatusState): boolean;
    authenticatedError(state: VaccinationStatusState): BannerError | undefined;
    authenticatedStatusMessage(state: VaccinationStatusState): string;
    authenticatedVaccineRecord(
        state: VaccinationStatusState
    ): CovidVaccineRecord | undefined;
    authenticatedVaccineRecordIsLoading(state: VaccinationStatusState): boolean;
    authenticatedVaccineRecordError(
        state: VaccinationStatusState
    ): BannerError | undefined;
    authenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState
    ): string;
    authenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState
    ): string;
}

type StoreContext = ActionContext<VaccinationStatusState, RootState>;
export interface VaccinationStatusActions
    extends ActionTree<VaccinationStatusState, RootState> {
    retrieveVaccineStatus(
        context: StoreContext,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<void>;
    retrievePublicVaccineRecord(
        context: StoreContext,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<CovidVaccineRecord>;
    handleError(context: StoreContext, error: ResultError): void;
    retrieveAuthenticatedVaccineStatus(
        context: StoreContext,
        params: {
            hdid: string;
        }
    ): Promise<void>;
    retrieveAuthenticatedVaccineRecord(
        context: StoreContext,
        params: {
            hdid: string;
        }
    ): Promise<CovidVaccineRecord>;
    handleAuthenticatedError(context: StoreContext, error: ResultError): void;
}

export interface VaccinationStatusMutations
    extends MutationTree<VaccinationStatusState> {
    setRequested(state: VaccinationStatusState): void;
    setVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ): void;
    vaccinationStatusError(
        state: VaccinationStatusState,
        error: BannerError
    ): void;
    setPdfRequested(state: VaccinationStatusState): void;
    pdfError(state: VaccinationStatusState, error: BannerError): void;
    setStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setPublicVaccineRecordRequested(state: VaccinationStatusState): void;
    setPublicVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setPublicVaccineRecord(
        state: VaccinationStatusState,
        vaccineRecord: CovidVaccineRecord
    ): void;
    setAuthenticatedRequested(state: VaccinationStatusState): void;
    setAuthenticatedVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ): void;
    authenticatedVaccinationStatusError(
        state: VaccinationStatusState,
        error: BannerError
    ): void;
    setAuthenticatedStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setAuthenticatedVaccineRecordRequested(state: VaccinationStatusState): void;
    setAuthenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setAuthenticatedVaccineRecord(
        state: VaccinationStatusState,
        vaccineRecord: CovidVaccineRecord
    ): void;
    setAuthenticatedVaccineRecordError(
        state: VaccinationStatusState,
        error: BannerError
    ): void;
    setAuthenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState,
        resultMessage: string
    ): void;
}

export interface VaccinationStatusModule
    extends Module<VaccinationStatusState, RootState> {
    namespaced: boolean;
    state: VaccinationStatusState;
    getters: VaccinationStatusGetters;
    actions: VaccinationStatusActions;
    mutations: VaccinationStatusMutations;
}
