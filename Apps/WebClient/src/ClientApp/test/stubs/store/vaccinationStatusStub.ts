import { voidMethod, voidPromise } from "@test/stubs/util";

import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import {
    VaccinationStatusActions,
    VaccinationStatusGetters,
    VaccinationStatusModule,
    VaccinationStatusMutations,
    VaccinationStatusState,
} from "@/store/modules/vaccinationStatus/types";

const vaccinationStatusState: VaccinationStatusState = {
    public: {
        vaccinationStatus: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    publicVaccineRecord: {
        vaccinationRecord: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    authenticated: {
        vaccinationStatus: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    authenticatedVaccineRecord: {
        vaccinationRecord: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
        resultMessage: "",
    },
};

const vaccinationStatusGetters: VaccinationStatusGetters = {
    vaccinationStatus(): VaccinationStatus | undefined {
        return undefined;
    },
    isLoading(): boolean {
        return false;
    },
    error(): BannerError | undefined {
        return undefined;
    },
    statusMessage(): string {
        return "";
    },
    publicVaccineRecordIsLoading(): boolean {
        return false;
    },
    publicVaccineRecordStatusMessage(): string {
        return "";
    },
    publicVaccineRecord(): CovidVaccineRecord | undefined {
        return undefined;
    },
    publicVaccineRecordError(): BannerError | undefined {
        return undefined;
    },
    authenticatedVaccinationStatus(): VaccinationStatus | undefined {
        return undefined;
    },
    authenticatedIsLoading(): boolean {
        return false;
    },
    authenticatedError(): BannerError | undefined {
        return undefined;
    },
    authenticatedStatusMessage(): string {
        return "";
    },
    authenticatedVaccineRecord(): CovidVaccineRecord | undefined {
        return undefined;
    },
    authenticatedVaccineRecordIsLoading(): boolean {
        return false;
    },
    authenticatedVaccineRecordError(): BannerError | undefined {
        return undefined;
    },
    authenticatedVaccineRecordStatusMessage(): string {
        return "";
    },
    authenticatedVaccineRecordResultMessage(): string {
        return "";
    },
};

const vaccinationStatusActions: VaccinationStatusActions = {
    retrieveVaccineStatus: voidPromise,
    retrieveVaccineStatusPdf: voidPromise,
    retrievePublicVaccineRecord: voidPromise,
    handleError: voidMethod,
    retrieveAuthenticatedVaccineStatus: voidPromise,
    retrieveAuthenticatedVaccineRecord: voidPromise,
    handleAuthenticatedError: voidMethod,
};

const vaccinationStatusMutations: VaccinationStatusMutations = {
    setRequested: voidMethod,
    setVaccinationStatus: voidMethod,
    vaccinationStatusError: voidMethod,
    setPdfRequested: voidMethod,
    setStatusMessage: voidMethod,
    setAuthenticatedRequested: voidMethod,
    setAuthenticatedVaccinationStatus: voidMethod,
    authenticatedVaccinationStatusError: voidMethod,
    setAuthenticatedStatusMessage: voidMethod,
    setAuthenticatedVaccineRecordRequested: voidMethod,
    setAuthenticatedVaccineRecordStatusMessage: voidMethod,
    setAuthenticatedVaccineRecord: voidMethod,
    setAuthenticatedVaccineRecordError: voidMethod,
    setAuthenticatedVaccineRecordResultMessage: voidMethod,
    setPublicVaccineRecordRequested: voidMethod,
    setPublicVaccineRecordStatusMessage: voidMethod,
    setPublicVaccineRecordError: voidMethod,
    setPublicVaccineRecord: voidMethod,
};

const vaccinationStatusStub: VaccinationStatusModule = {
    namespaced: true,
    state: vaccinationStatusState,
    getters: vaccinationStatusGetters,
    actions: vaccinationStatusActions,
    mutations: vaccinationStatusMutations,
};

export default vaccinationStatusStub;
