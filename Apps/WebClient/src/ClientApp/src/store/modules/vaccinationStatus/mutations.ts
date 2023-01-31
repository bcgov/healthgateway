import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationRecord from "@/models/vaccinationRecord";
import VaccinationStatus from "@/models/vaccinationStatus";

import { VaccinationStatusMutations, VaccinationStatusState } from "./types";

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
        const vaccinationRecords: Map<string, VaccinationRecord> =
            getVaccinationRecords(state);

        const vaccinationRecord: VaccinationRecord = {
            hdid: params.hdid,
            download: true,
            error: undefined,
            status: LoadStatus.REQUESTED,
            statusMessage: "",
            resultMessage: "",
        };

        vaccinationRecords.set(params.hdid, vaccinationRecord);
        state.authenticatedVaccineRecord.vaccinationRecords =
            vaccinationRecords;
        state.authenticatedVaccineRecord.activeHdid = params.hdid;
        state.authenticatedVaccineRecord.statusChanges++;
    },
    setAuthenticatedVaccineRecord(
        state: VaccinationStatusState,
        params: { hdid: string; vaccinationRecord: CovidVaccineRecord }
    ) {
        const vaccinationRecords: Map<string, VaccinationRecord> =
            getVaccinationRecords(state);

        const vaccinationRecord: VaccinationRecord | undefined =
            vaccinationRecords.get(params.hdid);

        if (vaccinationRecord !== undefined) {
            vaccinationRecord.record = params.vaccinationRecord;
            vaccinationRecord.status = LoadStatus.LOADED;
            vaccinationRecord.statusMessage = "";
            vaccinationRecord.error = undefined;

            state.authenticatedVaccineRecord.vaccinationRecords =
                vaccinationRecords;
        }
        state.authenticatedVaccineRecord.statusChanges++;
    },
    setAuthenticatedVaccineRecordError(
        state: VaccinationStatusState,
        params: { hdid: string; error: ResultError }
    ) {
        const vaccinationRecords: Map<string, VaccinationRecord> =
            getVaccinationRecords(state);

        const vaccinationRecord: VaccinationRecord | undefined =
            vaccinationRecords.get(params.hdid);

        if (vaccinationRecord !== undefined) {
            vaccinationRecord.error = params.error;
            vaccinationRecord.status = LoadStatus.ERROR;

            state.authenticatedVaccineRecord.vaccinationRecords =
                vaccinationRecords;
        }
        state.authenticatedVaccineRecord.statusChanges++;
    },
    setAuthenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        params: { hdid: string; statusMessage: string }
    ) {
        const vaccinationRecords: Map<string, VaccinationRecord> =
            getVaccinationRecords(state);

        const vaccinationRecord: VaccinationRecord | undefined =
            vaccinationRecords.get(params.hdid);

        if (vaccinationRecord !== undefined) {
            vaccinationRecord.statusMessage = params.statusMessage;

            state.authenticatedVaccineRecord.vaccinationRecords =
                vaccinationRecords;
        }
        state.authenticatedVaccineRecord.statusChanges++;
    },
    setAuthenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState,
        params: { hdid: string; resultMessage: string }
    ) {
        const vaccinationRecords: Map<string, VaccinationRecord> =
            getVaccinationRecords(state);

        const vaccinationRecord: VaccinationRecord | undefined =
            vaccinationRecords.get(params.hdid);

        if (vaccinationRecord !== undefined) {
            vaccinationRecord.resultMessage = params.resultMessage;

            state.authenticatedVaccineRecord.vaccinationRecords =
                vaccinationRecords;
        }
        state.authenticatedVaccineRecord.statusChanges++;
    },
    setAuthenticatedVaccineRecordDownload(
        state: VaccinationStatusState,
        params: { hdid: string; download: boolean }
    ) {
        const vaccinationRecords: Map<string, VaccinationRecord> =
            getVaccinationRecords(state);

        const vaccinationRecord: VaccinationRecord | undefined =
            vaccinationRecords.get(params.hdid);

        if (vaccinationRecord !== undefined) {
            vaccinationRecord.download = params.download;

            state.authenticatedVaccineRecord.vaccinationRecords =
                vaccinationRecords;
        }
        state.authenticatedVaccineRecord.statusChanges++;
    },
};

function getVaccinationRecords(
    state: VaccinationStatusState
): Map<string, VaccinationRecord> {
    if (
        state.authenticatedVaccineRecord.vaccinationRecords instanceof
        Map<string, VaccinationRecord>
    ) {
        return state.authenticatedVaccineRecord.vaccinationRecords;
    }

    return Object.assign(
        new Map<string, VaccinationRecord>(),
        state.authenticatedVaccineRecord.vaccinationRecords
    );
}
