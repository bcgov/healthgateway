import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Action, Text } from "@/constants/tracking";
import { LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import {
    type PatientData,
    type PatientDataFile,
    type PatientDataResponse,
    PatientDataToHealthDataTypeMap,
    PatientDataType,
} from "@/models/patientData";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { patientDataTypeToEntryTypeMap } from "@/services/patientData.service";
import { getDataset } from "@/utils/eventData";

import { useErrorStore } from "./error.store";
import { inProgressCalls } from "./utils/inProgressCalls";

interface PatientDataState {
    /** records per data type; a key exists once that type has been loaded */
    data: Partial<Record<PatientDataType, PatientData[]>>;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}
interface PatientDataFileState {
    data?: PatientDataFile;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}

const defaultState: PatientDataState = {
    data: {},
    status: LoadStatus.NONE,
    statusMessage: "",
};
const defaultFileState: PatientDataFileState = {
    status: LoadStatus.NONE,
    statusMessage: "",
};

function getErrorSource(patientDataType: PatientDataType): ErrorSourceType {
    switch (patientDataType) {
        case PatientDataType.DiagnosticImaging:
            return ErrorSourceType.DiagnosticImaging;
        case PatientDataType.OrganDonorRegistrationStatus:
            return ErrorSourceType.OrganDonorRegistration;
        case PatientDataType.BcCancerScreening:
            return ErrorSourceType.BcCancerScreening;
        default:
            return ErrorSourceType.PatientData;
    }
}

function trackDataLoaded(
    patientDataTypes: PatientDataType[],
    data: PatientDataResponse
) {
    for (const patientDataType of patientDataTypes) {
        const dataSet = data.items.filter(
            (i) =>
                i.type === PatientDataToHealthDataTypeMap.get(patientDataType)
        );
        const entryType = patientDataTypeToEntryTypeMap.get(patientDataType);
        if (dataSet.length > 0 && entryType !== undefined)
            services.tracking.trackEvent({
                action: Action.Load,
                text: Text.Data,
                dataset: getDataset(entryType),
            });
    }
}

interface PatientDataStoreState {
    states: Record<string, PatientDataState>;
    files: Record<string, PatientDataFileState>;
    retrievePatientData: (
        hdid: string,
        patientDataTypes: PatientDataType[]
    ) => Promise<PatientData[]>;
    retrievePatientDataFile: (
        hdid: string,
        fileId: string
    ) => Promise<PatientDataFile>;
    reset: () => void;
}

const records = (
    state: PatientDataState | undefined,
    types: PatientDataType[]
) => types.flatMap((t) => state?.data[t] ?? []);

/** Port of stores/patientData.ts */
export const usePatientDataStore = create<PatientDataStoreState>((set, get) => {
    const inProgress = inProgressCalls();
    let running: {
        hdid: string;
        types: PatientDataType[];
        call: Promise<unknown>;
    }[] = [];
    const stateOf = (hdid: string) => get().states[hdid] ?? defaultState;
    const putState = (hdid: string, change: Partial<PatientDataState>) =>
        set((s) => ({
            states: {
                ...s.states,
                [hdid]: { ...(s.states[hdid] ?? defaultState), ...change },
            },
        }));
    const putFile = (fileId: string, change: Partial<PatientDataFileState>) =>
        set((s) => ({
            files: {
                ...s.files,
                [fileId]: {
                    ...(s.files[fileId] ?? defaultFileState),
                    ...change,
                },
            },
        }));

    function handleError(
        error: unknown,
        errorType: ErrorType,
        patientDataTypes: PatientDataType[],
        hdid: string,
        fileId?: string
    ) {
        logger.error(`ERROR: ${describeError(error)}`);
        const resultError = error instanceof ResultError ? error : undefined;
        const errorState = {
            error: resultError,
            statusMessage: resultError?.message ?? "Error occurred",
            status: LoadStatus.ERROR,
        };
        if (fileId) putFile(fileId, errorState);
        else putState(hdid, errorState);
        const errorStore = useErrorStore.getState();
        if (resultError?.statusCode === 429)
            errorStore.setTooManyRequestsWarning("page");
        else if (patientDataTypes.length > 0)
            for (const t of patientDataTypes)
                errorStore.addError(
                    errorType,
                    getErrorSource(t),
                    resultError?.traceId
                );
        else
            errorStore.addError(
                errorType,
                ErrorSourceType.PatientData,
                resultError?.traceId
            );
    }

    return {
        states: {},
        files: {},
        retrievePatientData: async (hdid, patientDataTypes) => {
            const state = stateOf(hdid);
            if (
                state.status === LoadStatus.LOADED &&
                patientDataTypes.every((t) => t in state.data)
            ) {
                logger.debug("Patient data found stored, not querying!");
                return records(state, patientDataTypes);
            }
            // a call already asking for at least these types answers this one too
            const covering = running.find(
                (r) =>
                    r.hdid === hdid &&
                    patientDataTypes.every((t) => r.types.includes(t))
            );
            if (covering) {
                await covering.call;
                return records(stateOf(hdid), patientDataTypes);
            }
            const entry = {
                hdid,
                types: patientDataTypes,
                call: inProgress.share(
                    `${hdid}:${patientDataTypes.join()}`,
                    async () => {
                        putState(hdid, { status: LoadStatus.REQUESTED });
                        try {
                            const data =
                                await services.patientData.getPatientData(
                                    hdid,
                                    patientDataTypes
                                );
                            const next = { ...stateOf(hdid).data };
                            for (const t of patientDataTypes)
                                next[t] = data.items.filter(
                                    (d) =>
                                        d.type ===
                                        PatientDataToHealthDataTypeMap.get(t)
                                );
                            putState(hdid, {
                                data: next,
                                error: undefined,
                                statusMessage: "success",
                                status: LoadStatus.LOADED,
                            });
                            trackDataLoaded(patientDataTypes, data);
                            return data.items;
                        } catch (error) {
                            rethrowIfSessionEnded(error);
                            handleError(
                                error,
                                ErrorType.Retrieve,
                                patientDataTypes,
                                hdid
                            );
                            throw error;
                        }
                    }
                ),
            };
            running.push(entry);
            return entry.call.finally(() => {
                running = running.filter((r) => r !== entry);
            });
        },
        retrievePatientDataFile: async (hdid, fileId) => {
            const fileState = get().files[fileId];
            if (
                fileState?.status === LoadStatus.LOADED &&
                fileState.data !== undefined
            ) {
                logger.debug("Patient data file found stored, not querying!");
                return fileState.data;
            }
            putFile(fileId, { status: LoadStatus.REQUESTED });
            try {
                const data = await services.patientData.getFile(hdid, fileId);
                putFile(fileId, {
                    data,
                    error: undefined,
                    statusMessage: "success",
                    status: LoadStatus.LOADED,
                });
                return data;
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Retrieve, [], hdid, fileId);
                throw error;
            }
        },
        reset: () => {
            inProgress.clear();
            running = [];
            set({ states: {}, files: {} });
        },
    };
});

/** the hdid's patient data state (pick records with `patientDataRecords`) */
export const selectPatientDataState =
    (hdid: string) => (s: PatientDataStoreState) =>
        s.states[hdid];
export const patientDataRecords = records;
export const selectPatientDataAreLoading =
    (hdid: string) => (s: PatientDataStoreState) =>
        s.states[hdid]?.status === LoadStatus.REQUESTED;
export const selectPatientDataFileLoading =
    (fileId?: string) => (s: PatientDataStoreState) =>
        fileId !== undefined &&
        s.files[fileId]?.status === LoadStatus.REQUESTED;
