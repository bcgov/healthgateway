import { CustomBannerError } from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";

import { ResultError } from "./../../../models/requestResult";
import { VaccinationStatusGetters, VaccinationStatusState } from "./types";

export const getters: VaccinationStatusGetters = {
    publicVaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined {
        return state.public.vaccinationStatus;
    },
    publicIsLoading(state: VaccinationStatusState): boolean {
        return state.public.status === LoadStatus.REQUESTED;
    },
    publicError(state: VaccinationStatusState): CustomBannerError | undefined {
        return state.public.error;
    },
    publicStatusMessage(state: VaccinationStatusState): string {
        return state.public.statusMessage;
    },
    publicVaccineRecord(
        state: VaccinationStatusState
    ): CovidVaccineRecord | undefined {
        return state.publicVaccineRecord.vaccinationRecord;
    },
    publicVaccineRecordIsLoading(state: VaccinationStatusState): boolean {
        return state.publicVaccineRecord.status === LoadStatus.REQUESTED;
    },
    publicVaccineRecordError(
        state: VaccinationStatusState
    ): CustomBannerError | undefined {
        return state.publicVaccineRecord.error;
    },
    publicVaccineRecordStatusMessage(state: VaccinationStatusState): string {
        return state.publicVaccineRecord.statusMessage;
    },
    authenticatedVaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined {
        return state.authenticated.vaccinationStatus;
    },
    authenticatedIsLoading(state: VaccinationStatusState): boolean {
        return state.authenticated.status === LoadStatus.REQUESTED;
    },
    authenticatedError(state: VaccinationStatusState): ResultError | undefined {
        return state.authenticated.error;
    },
    authenticatedStatusMessage(state: VaccinationStatusState): string {
        return state.authenticated.statusMessage;
    },
    authenticatedVaccineRecord(
        state: VaccinationStatusState
    ): CovidVaccineRecord | undefined {
        return state.authenticatedVaccineRecord.vaccinationRecord;
    },
    authenticatedVaccineRecordIsLoading(
        state: VaccinationStatusState
    ): boolean {
        return state.authenticatedVaccineRecord.status === LoadStatus.REQUESTED;
    },
    authenticatedVaccineRecordError(
        state: VaccinationStatusState
    ): ResultError | undefined {
        return state.authenticatedVaccineRecord.error;
    },
    authenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState
    ): string {
        return state.authenticatedVaccineRecord.statusMessage;
    },
    authenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState
    ): string {
        return state.authenticatedVaccineRecord.resultMessage;
    },
};
