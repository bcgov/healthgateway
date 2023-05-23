import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import VaccineRecordState from "@/models/vaccineRecordState";

import { VaccinationStatusMutations, VaccinationStatusState } from "./types";
import {
    getAuthenticatedVaccineRecordState,
    setAuthenticatedVaccineRecordState,
} from "./util";

export const mutations: VaccinationStatusMutations = {
    setPublicRequested(state: VaccinationStatusState) {
        state.public.error = undefined;
        state.public.status = LoadStatus.REQUESTED;
        state.public.statusMessage = "";
    },
    setPublicVaccinationStatus(
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
    publicVaccinationStatusError(
        state: VaccinationStatusState,
        error: CustomBannerError
    ) {
        state.public.vaccinationStatus = undefined;
        state.public.error = error;
        state.public.status = LoadStatus.ERROR;
        state.public.statusMessage = "";
    },
    setPublicStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ) {
        state.public.statusMessage = statusMessage;
    },
    setPublicVaccineRecordRequested(state: VaccinationStatusState) {
        state.publicVaccineRecord.status = LoadStatus.REQUESTED;
        state.publicVaccineRecord.statusMessage = "";
        state.publicVaccineRecord.error = undefined;
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
    setPublicVaccineRecordError(
        state: VaccinationStatusState,
        error: CustomBannerError
    ) {
        state.publicVaccineRecord.error = error;
        state.publicVaccineRecord.status = LoadStatus.ERROR;
    },
    setPublicVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ) {
        state.publicVaccineRecord.statusMessage = statusMessage;
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
        error: ResultError
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
    setAuthenticatedVaccineRecordRequested(
        state: VaccinationStatusState,
        params: { hdid: string }
    ) {
        const { hdid } = params;
        const currentState = getAuthenticatedVaccineRecordState(state, hdid);
        const nextState: VaccineRecordState = {
            ...currentState,
            record: undefined,
            download: true,
            error: undefined,
            status: LoadStatus.REQUESTED,
            statusMessage: "",
            resultMessage: "",
        };
        setAuthenticatedVaccineRecordState(state, hdid, nextState);
    },
    setAuthenticatedVaccineRecord(
        state: VaccinationStatusState,
        params: { hdid: string; record: CovidVaccineRecord }
    ) {
        const { hdid, record } = params;
        const currentState = getAuthenticatedVaccineRecordState(state, hdid);
        const nextState: VaccineRecordState = {
            ...currentState,
            record,
            error: undefined,
            status: LoadStatus.LOADED,
            statusMessage: "",
        };
        setAuthenticatedVaccineRecordState(state, hdid, nextState);
    },
    setAuthenticatedVaccineRecordError(
        state: VaccinationStatusState,
        params: { hdid: string; error: ResultError }
    ) {
        const { hdid, error } = params;
        const currentState = getAuthenticatedVaccineRecordState(state, hdid);
        const nextState: VaccineRecordState = {
            ...currentState,
            record: undefined,
            error,
            status: LoadStatus.ERROR,
            statusMessage: "",
        };
        setAuthenticatedVaccineRecordState(state, hdid, nextState);
    },
    setAuthenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        params: { hdid: string; statusMessage: string }
    ) {
        const { hdid, statusMessage } = params;
        const currentState = getAuthenticatedVaccineRecordState(state, hdid);
        const nextState: VaccineRecordState = {
            ...currentState,
            statusMessage,
        };
        setAuthenticatedVaccineRecordState(state, hdid, nextState);
    },
    setAuthenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState,
        params: { hdid: string; resultMessage: string }
    ) {
        const { hdid, resultMessage } = params;
        const currentState = getAuthenticatedVaccineRecordState(state, hdid);
        const nextState: VaccineRecordState = {
            ...currentState,
            resultMessage,
        };
        setAuthenticatedVaccineRecordState(state, hdid, nextState);
    },
    setAuthenticatedVaccineRecordDownload(
        state: VaccinationStatusState,
        params: { hdid: string; download: boolean }
    ) {
        const { hdid, download } = params;
        const currentState = getAuthenticatedVaccineRecordState(state, hdid);
        const nextState: VaccineRecordState = {
            ...currentState,
            download,
        };
        setAuthenticatedVaccineRecordState(state, hdid, nextState);
    },
};
