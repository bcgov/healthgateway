import { defineStore } from "pinia";
import { ref } from "vue";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { MedicationState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import MedicationStatement from "@/models/medicationStatement";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Dataset, Text } from "@/plugins/extensions";
import {
    ILogger,
    IMedicationService,
    ITrackingService,
} from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";

const defaultMedicationState: MedicationState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    protectiveWordAttempts: 0,
};

export const useMedicationStore = defineStore("medication", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const medicationService = container.get<IMedicationService>(
        SERVICE_IDENTIFIER.MedicationService
    );
    const datasetMapUtil = new DatasetMapUtils<
        MedicationStatement[],
        MedicationState
    >(defaultMedicationState);

    const errorStore = useErrorStore();

    const medicationMap = ref(new Map<string, MedicationState>());

    function formatDinPin(din: string | null | undefined): string {
        return (din ?? "").padStart(8, "0");
    }

    function getMedicationState(hdid: string): MedicationState {
        return datasetMapUtil.getDatasetState(medicationMap.value, hdid);
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
        medicationResult: RequestResult<MedicationStatement[]>
    ): void {
        if (medicationResult.resultStatus == ResultType.Success) {
            const payload = medicationResult.resourcePayload ?? [];

            // Normalize DIN/PIN
            for (const medication of payload) {
                if (medication?.medicationSummary) {
                    medication.medicationSummary.din = formatDinPin(
                        medication.medicationSummary.din
                    );
                }
            }

            datasetMapUtil.setStateData(medicationMap.value, hdid, payload, {
                protectiveWordAttempts: 0,
            });
        } else if (
            medicationResult.resultStatus == ResultType.ActionRequired &&
            medicationResult.resultError?.actionCode == ActionType.Protected
        ) {
            const currentState = getMedicationState(hdid);
            datasetMapUtil.setStateData(
                medicationMap.value,
                hdid,
                medicationResult.resourcePayload ?? [],
                {
                    protectiveWordAttempts:
                        currentState.protectiveWordAttempts + 1,
                    status: LoadStatus.PROTECTED,
                }
            );
        } else {
            const resultError = medicationResult.resultError
                ? ResultError.fromModel(medicationResult.resultError)
                : new ResultError(
                      "MedicationStore",
                      "Unknown API error when settings medications."
                  );
            datasetMapUtil.setStateError(
                medicationMap.value,
                hdid,
                resultError,
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
        datasetMapUtil.setStateError(medicationMap.value, hdid, error);
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
    ): Promise<RequestResult<MedicationStatement[]>> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getMedicationState(hdid).status === LoadStatus.LOADED) {
            logger.debug("Medications found stored, not querying!");
            const medicationsData: MedicationStatement[] = medications(hdid);
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: medicationsData,
                resultStatus: ResultType.Success,
                totalResultCount: medicationsData.length,
            });
        }

        logger.debug("Retrieving medications");
        datasetMapUtil.setStateRequested(medicationMap.value, hdid);
        return medicationService
            .getPatientMedicationStatements(hdid, protectiveWord)
            .then((result) => {
                if (result.resultStatus === ResultType.Error) {
                    throw result.resultError
                        ? ResultError.fromModel(result.resultError)
                        : new ResultError(
                              "MedicationStore",
                              "Unknown API error when retrieving medications."
                          );
                }

                if (
                    result.resultStatus === ResultType.Success &&
                    result.resourcePayload.length > 0
                ) {
                    trackingService.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.Medications,
                    });
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
});
