import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";

import { VaccinationStatusMutations, VaccinationStatusState } from "./types";

export const mutations: VaccinationStatusMutations = {
    setRequested(state: VaccinationStatusState) {
        state.public.error = undefined;
        state.public.status = LoadStatus.REQUESTED;
        state.public.statusMessage = "";
    },
    setVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ) {
        state.public.vaccinationStatus = {
            ...vaccinationStatus,
            issueddate: new DateWrapper().toISO(),
        };
        state.public.status = LoadStatus.LOADED;
        state.public.statusMessage = "";
    },
    setPublicVaccineRecordRequested(state: VaccinationStatusState) {
        state.publicVaccineRecord.status = LoadStatus.REQUESTED;
        state.publicVaccineRecord.statusMessage = "";
        state.publicVaccineRecord.error = undefined;
    },
    setPublicVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ) {
        state.publicVaccineRecord.statusMessage = statusMessage;
    },
    setPublicVaccineRecord(
        state: VaccinationStatusState,
        vaccineRecord: CovidVaccineRecord
    ) {
        state.publicVaccineRecord.vaccinationRecord = vaccineRecord;
        state.publicVaccineRecord.status = LoadStatus.LOADED;
        state.publicVaccineRecord.statusMessage = "";
        state.publicVaccineRecord.error = undefined;
    },
    vaccinationStatusError(state: VaccinationStatusState, error: BannerError) {
        state.public.vaccinationStatus = undefined;
        state.public.error = error;
        state.public.status = LoadStatus.ERROR;
        state.public.statusMessage = "";
    },
    setPdfRequested(state: VaccinationStatusState) {
        state.public.error = undefined;
        state.public.statusMessage = "";
    },
    setPublicVaccineRecordError(
        state: VaccinationStatusState,
        error: BannerError
    ) {
        state.publicVaccineRecord.error = error;
        state.publicVaccineRecord.status = LoadStatus.ERROR;
    },
    setStatusMessage(state: VaccinationStatusState, statusMessage: string) {
        state.public.statusMessage = statusMessage;
    },
    setAuthenticatedRequested(state: VaccinationStatusState) {
        state.authenticated.error = undefined;
        state.authenticated.status = LoadStatus.REQUESTED;
        state.authenticated.statusMessage = "";
    },
    setAuthenticatedVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ) {
        state.authenticated.vaccinationStatus = {
            ...vaccinationStatus,
            issueddate: new DateWrapper().toISO(),
        };
        state.authenticated.status = LoadStatus.LOADED;
        state.authenticated.statusMessage = "";
    },
    authenticatedVaccinationStatusError(
        state: VaccinationStatusState,
        error: BannerError
    ) {
        state.authenticated.vaccinationStatus = undefined;
        state.authenticated.error = error;
        state.authenticated.status = LoadStatus.ERROR;
        state.authenticated.statusMessage = "";
    },
    setAuthenticatedStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ) {
        state.authenticated.statusMessage = statusMessage;
    },
    setAuthenticatedVaccineRecordRequested(state: VaccinationStatusState) {
        state.authenticatedVaccineRecord.error = undefined;
        state.authenticatedVaccineRecord.status = LoadStatus.REQUESTED;
        state.authenticatedVaccineRecord.statusMessage = "";
        state.authenticatedVaccineRecord.resultMessage = "";
    },
    setAuthenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ) {
        state.authenticatedVaccineRecord.statusMessage = statusMessage;
    },
    setAuthenticatedVaccineRecord(
        state: VaccinationStatusState,
        vaccineRecord: CovidVaccineRecord
    ) {
        state.authenticatedVaccineRecord.vaccinationRecord = vaccineRecord;
        state.authenticatedVaccineRecord.status = LoadStatus.LOADED;
        state.authenticatedVaccineRecord.statusMessage = "";
        state.authenticatedVaccineRecord.error = undefined;
    },
    setAuthenticatedVaccineRecordError(
        state: VaccinationStatusState,
        error: BannerError
    ) {
        state.authenticatedVaccineRecord.error = error;
        state.authenticatedVaccineRecord.status = LoadStatus.ERROR;
    },
    setAuthenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState,
        resultMessage: string
    ) {
        state.authenticatedVaccineRecord.resultMessage = resultMessage;
    },
};
