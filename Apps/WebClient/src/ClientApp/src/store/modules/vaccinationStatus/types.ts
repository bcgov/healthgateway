import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { VaccineProofTemplate } from "@/constants/vaccineProofTemplate";
import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import Report from "@/models/report";
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
    retrieveVaccineStatusPdf(
        context: StoreContext,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
            proofTemplate: VaccineProofTemplate;
        }
    ): Promise<Report>;
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
            proofTemplate: VaccineProofTemplate;
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
}

export interface VaccinationStatusModule
    extends Module<VaccinationStatusState, RootState> {
    namespaced: boolean;
    state: VaccinationStatusState;
    getters: VaccinationStatusGetters;
    actions: VaccinationStatusActions;
    mutations: VaccinationStatusMutations;
}
