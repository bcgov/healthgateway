import { defineStore } from "pinia";
import { MedicationState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";
import { ref } from "vue";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import { container } from "@/ioc/container";
import { ILogger, IMedicationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useErrorStore } from "@/stores/error";
import EventTracker from "@/utility/eventTracker";
import { EntryType } from "@/constants/entryType";

const defaultMedicationState: MedicationState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    protectiveWordAttempts: 0,
};

export const useMedicationStatementStore = defineStore(
    "medicationStatement",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const medicationService = container.get<IMedicationService>(
            SERVICE_IDENTIFIER.MedicationService
        );
        const datasetMapUtil = new DatasetMapUtils<
            MedicationStatementHistory[],
            MedicationState
        >(defaultMedicationState);

        const errorStore = useErrorStore();

        const medicationsMap = ref(new Map<string, MedicationState>());

        function getMedicationState(hdid: string): MedicationState {
            return datasetMapUtil.getDatasetState(medicationsMap.value, hdid);
        }

        function medications(hdid: string) {
            return getMedicationState(hdid).data;
        }

        function medicationsCount(hdid: string) {
            return getMedicationState(hdid).data.length;
        }

        function medicationsAreLoading(hdid: string) {
            return getMedicationState(hdid).status === LoadStatus.REQUESTED;
        }

        function medicationsAreProtected(hdid: string) {
            return getMedicationState(hdid).status === LoadStatus.PROTECTED;
        }

        function protectiveWordAttempts(hdid: string) {
            return getMedicationState(hdid).protectiveWordAttempts;
        }

        function setMedications(
            hdid: string,
            medicationResult: RequestResult<MedicationStatementHistory[]>
        ): void {
            if (medicationResult.resultStatus == ResultType.Success) {
                datasetMapUtil.setStateData(
                    medicationsMap.value,
                    hdid,
                    medicationResult.resourcePayload,
                    {
                        protectiveWordAttempts: 0,
                    }
                );
            } else if (
                medicationResult.resultStatus == ResultType.ActionRequired &&
                medicationResult.resultError?.actionCode == ActionType.Protected
            ) {
                const currentState = getMedicationState(hdid);
                datasetMapUtil.setStateData(
                    medicationsMap.value,
                    hdid,
                    medicationResult.resourcePayload,
                    {
                        protectiveWordAttempts:
                            currentState.protectiveWordAttempts + 1,
                        status: LoadStatus.PROTECTED,
                    }
                );
            } else {
                datasetMapUtil.setStateError(
                    medicationsMap.value,
                    hdid,
                    medicationResult.resultError,
                    "Error returned from the medications call"
                );
            }
        }

        function handleError(
            hdid: string,
            error: ResultError,
            errorType: ErrorType
        ) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);
            datasetMapUtil.setStateError(medicationsMap.value, hdid, error);
            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    errorType,
                    ErrorSourceType.MedicationStatements,
                    error.traceId
                );
            }
        }

        function retrieveMedications(
            hdid: string,
            protectiveWord?: string
        ): Promise<RequestResult<MedicationStatementHistory[]>> {
            if (getMedicationState(hdid).status === LoadStatus.LOADED) {
                logger.debug("Medications found stored, not querying!");
                const medicationsData: MedicationStatementHistory[] =
                    medications(hdid);
                return Promise.resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: medicationsData,
                    resultStatus: ResultType.Success,
                    totalResultCount: medicationsData.length,
                });
            }

            logger.debug("Retrieving medications");
            datasetMapUtil.setStateRequested(medicationsMap.value, hdid);
            return medicationService
                .getPatientMedicationStatementHistory(hdid, protectiveWord)
                .then((result) => {
                    if (result.resultStatus === ResultType.Error) {
                        throw result.resultError;
                    }

                    if (result.resultStatus === ResultType.Success) {
                        EventTracker.loadData(
                            EntryType.Medication,
                            result.resourcePayload.length
                        );
                    }
                    setMedications(hdid, result);
                    return result;
                })
                .catch((error: ResultError) => {
                    handleError(hdid, error, ErrorType.Retrieve);
                    throw error;
                });
        }

        return {
            medications,
            medicationsCount,
            medicationsAreLoading,
            medicationsAreProtected,
            protectiveWordAttempts,
            retrieveMedications,
        };
    }
);
