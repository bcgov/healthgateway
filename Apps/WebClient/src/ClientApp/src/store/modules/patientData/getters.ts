import {
    PatientData,
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataGetters,
    PatientDataState,
} from "@/store/modules/patientData/types";
import {
    getPatientDataFileState,
    getPatientDataRecordState,
} from "@/store/modules/patientData/utils";

export const getters: PatientDataGetters = {
    isPatientDataFileLoading(
        state: PatientDataState
    ): (fileId: string) => boolean {
        return (fileId: string) =>
            getPatientDataFileState(state, fileId).status ===
            LoadStatus.REQUESTED;
    },
    patientDataFile(
        state: PatientDataState
    ): (fileId: string) => PatientDataFile | undefined {
        return function (fileId: string) {
            return getPatientDataFileState(state, fileId).data;
        };
    },
    patientData(
        state: PatientDataState
    ): (hdid: string, patientDataTypes: PatientDataType[]) => PatientData[] {
        return (hdid: string, patientDataTypes: PatientDataType[]) => {
            const records: PatientData[] = [];
            const data = getPatientDataRecordState(state, hdid).data;
            if (data) {
                patientDataTypes.forEach((patientDataType) => {
                    const patientData = data[patientDataType];
                    if (patientData) {
                        records.push(...patientData);
                    }
                });
            }
            return records;
        };
    },
    patientDataAreLoading(state: PatientDataState): (hdid: string) => boolean {
        return (hdid: string) =>
            getPatientDataRecordState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
    patientDataCount(
        _state: PatientDataState,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        getters: any
    ): (hdid: string, patientDataTypes: PatientDataType[]) => number {
        return (hdid: string, patientDataTypes: PatientDataType[]) => {
            const data = getters.patientData(hdid, patientDataTypes);
            return data.length;
        };
    },
};
