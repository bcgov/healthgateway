﻿import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    PatientDataFileState,
    PatientDataMap,
    PatientDataState,
} from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import PatientDataResponse, {
    PatientData,
    PatientDataFile,
    PatientDataToHealthDataTypeMap,
    PatientDataType,
} from "@/models/patientDataResponse";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Text } from "@/plugins/extensions";
import {
    ILogger,
    IPatientDataService,
    ITrackingService,
} from "@/services/interfaces";
import { patientDataTypeToEntryTypeMap } from "@/services/restPatientDataService";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";
import EventDataUtility from "@/utility/eventDataUtility";

const defaultPatientDataState: PatientDataState = {
    data: new Map<PatientDataType, PatientData[]>(),
    statusMessage: "",
    status: LoadStatus.NONE,
    error: undefined,
};

const defaultPatientDataFileState: PatientDataFileState = {
    data: undefined,
    statusMessage: "",
    status: LoadStatus.NONE,
    error: undefined,
};

function reportDataLoaded(
    patientDataTypes: PatientDataType[],
    data: PatientDataResponse
) {
    const trackingService = container.get<ITrackingService>(
        SERVICE_IDENTIFIER.TrackingService
    );

    for (const patientDataType of patientDataTypes) {
        const dataSet = data.items.filter(
            (i) =>
                i.type === PatientDataToHealthDataTypeMap.get(patientDataType)
        );
        const entryType = patientDataTypeToEntryTypeMap.get(patientDataType);
        if (dataSet.length > 0 && entryType !== undefined) {
            trackingService.trackEvent({
                action: Action.Load,
                text: Text.Data,
                dataset: EventDataUtility.getDataset(entryType),
            });
        }
    }
}

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

export const usePatientDataStore = defineStore("patientData", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const patientDataService = container.get<IPatientDataService>(
        SERVICE_IDENTIFIER.PatientDataService
    );
    const patientDataMapUtil = new DatasetMapUtils<
        PatientDataMap,
        PatientDataState
    >(defaultPatientDataState);
    const patientDataFileMapUtil = new DatasetMapUtils<
        PatientDataFile | undefined,
        PatientDataFileState
    >(defaultPatientDataFileState);

    const errorStore = useErrorStore();

    const patientDataMap = ref(new Map<string, PatientDataState>());
    const patientDataFilesMap = ref(new Map<string, PatientDataFileState>());

    function getPatientDataState(hdid: string): PatientDataState {
        return patientDataMapUtil.getDatasetState(patientDataMap.value, hdid);
    }

    function getPatientDataFileState(fileId: string): PatientDataFileState {
        return patientDataFileMapUtil.getDatasetState(
            patientDataFilesMap.value,
            fileId
        );
    }

    function patientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): PatientData[] {
        const records: PatientData[] = [];
        const data = getPatientDataState(hdid).data;
        if (data) {
            for (const patientDataType of patientDataTypes) {
                records.push(...(data.get(patientDataType) ?? []));
            }
        }
        return records;
    }

    function patientDataCount(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): number {
        return patientData(hdid, patientDataTypes).length;
    }

    function patientDataAreLoading(hdid: string): boolean {
        return getPatientDataState(hdid)?.status === LoadStatus.REQUESTED;
    }

    function patientDataFile(fileId: string): PatientDataFile | undefined {
        return getPatientDataFileState(fileId).data;
    }

    function isPatientDataFileLoading(fileId: string): boolean {
        return getPatientDataFileState(fileId).status === LoadStatus.REQUESTED;
    }

    function setPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[],
        data: PatientData[]
    ): void {
        const patientDataStateData = getPatientDataState(hdid).data;

        for (const patientDataType of patientDataTypes) {
            patientDataStateData.set(
                patientDataType,
                data.filter(
                    (d) =>
                        d.type ===
                        PatientDataToHealthDataTypeMap.get(patientDataType)
                )
            );
        }

        patientDataMapUtil.setStateData(
            patientDataMap.value,
            hdid,
            patientDataStateData
        );
    }

    function areAllPatientDataTypesStored(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): boolean {
        const patientDataState = getPatientDataState(hdid);
        if (!patientDataState.data) {
            return false;
        }
        // Return false if any of the data types has a key registered.
        return !patientDataTypes.some(
            (patientDataType) => !patientDataState.data.has(patientDataType)
        );
    }

    function handleError(
        error: ResultError,
        errorType: ErrorType,
        patientDataTypes: PatientDataType[],
        hdid: string,
        fileId?: string
    ): void {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        if (fileId) {
            patientDataFileMapUtil.setStateError(
                patientDataFilesMap.value,
                fileId,
                error
            );
        } else {
            patientDataMapUtil.setStateError(patientDataMap.value, hdid, error);
        }

        if (error.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else if (patientDataTypes && patientDataTypes.length > 0) {
            patientDataTypes.forEach((patientDataType) => {
                errorStore.addError(
                    errorType,
                    getErrorSource(patientDataType),
                    error.traceId
                );
            });
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.PatientData,
                error.traceId
            );
        }
    }

    function retrievePatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientData[]> {
        if (
            getPatientDataState(hdid).status === LoadStatus.LOADED &&
            areAllPatientDataTypesStored(hdid, patientDataTypes)
        ) {
            logger.debug("Patient data found stored, not querying!");
            return Promise.resolve(patientData(hdid, patientDataTypes));
        }
        patientDataMapUtil.setStateRequested(patientDataMap.value, hdid);
        return patientDataService
            .getPatientData(hdid, patientDataTypes)
            .then((data) => {
                setPatientData(hdid, patientDataTypes, data.items);
                reportDataLoaded(patientDataTypes, data);
                return data.items;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Retrieve, patientDataTypes, hdid);
                throw error;
            });
    }

    function retrievePatientDataFile(
        hdid: string,
        fileId: string
    ): Promise<PatientDataFile> {
        const fileState = getPatientDataFileState(fileId);
        if (
            fileState.status === LoadStatus.LOADED &&
            fileState.data !== undefined
        ) {
            logger.debug("Patient data file found stored, not querying!");
            return Promise.resolve(patientDataFile(fileId)!);
        }
        patientDataFileMapUtil.setStateRequested(
            patientDataFilesMap.value,
            fileId
        );
        return patientDataService
            .getFile(hdid, fileId)
            .then((data) => {
                patientDataFileMapUtil.setStateData(
                    patientDataFilesMap.value,
                    fileId,
                    data
                );
                return data;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Retrieve, [], hdid, fileId);
                throw error;
            });
    }

    return {
        patientDataMap,
        patientDataFilesMap,
        patientData,
        patientDataCount,
        patientDataAreLoading,
        patientDataFile,
        isPatientDataFileLoading,
        retrievePatientData,
        retrievePatientDataFile,
    };
});
