import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";

import { VaccinationStatusGetters, VaccinationStatusState } from "./types";

export const getters: VaccinationStatusGetters = {
    vaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined {
        return state.public.vaccinationStatus;
    },
    isLoading(state: VaccinationStatusState): boolean {
        return state.public.status === LoadStatus.REQUESTED;
    },
    error(state: VaccinationStatusState): BannerError | undefined {
        return state.public.error;
    },
    publicVaccineRecordError(
        state: VaccinationStatusState
    ): BannerError | undefined {
        return state.publicVaccineRecord.error;
    },
    statusMessage(state: VaccinationStatusState): string {
        return state.public.statusMessage;
    },
    publicVaccineRecordIsLoading(state: VaccinationStatusState): boolean {
        return state.publicVaccineRecord.status === LoadStatus.REQUESTED;
    },
    publicVaccineRecord(
        state: VaccinationStatusState
    ): CovidVaccineRecord | undefined {
        return state.publicVaccineRecord.vaccinationRecord;
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
    authenticatedError(state: VaccinationStatusState): BannerError | undefined {
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
    ): BannerError | undefined {
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
