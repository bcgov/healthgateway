import { defineStore } from "pinia";
import { SpecialAuthorityRequestState } from "@/models/datasetState";
import { ref } from "vue";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";
import MedicationRequest from "@/models/medicationRequest";
import { LoadStatus } from "@/models/storeOperations";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger, ISpecialAuthorityService } from "@/services/interfaces";
import { container } from "@/ioc/container";
import { useErrorStore } from "@/stores/error";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import EventTracker from "@/utility/eventTracker";
import { EntryType } from "@/constants/entryType";

const defaultSpecialAuthorityRequestState: SpecialAuthorityRequestState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};
export const useSpecialAuthorityRequestStore = defineStore(
    "specialAuthorityRequest",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const specialAuthorityService = container.get<ISpecialAuthorityService>(
            SERVICE_IDENTIFIER.SpecialAuthorityService
        );
        const datasetMapUtil = new DatasetMapUtils<
            MedicationRequest[],
            SpecialAuthorityRequestState
        >(defaultSpecialAuthorityRequestState);

        const errorStore = useErrorStore();

        const medicationRequestStateMap = ref(
            new Map<string, SpecialAuthorityRequestState>()
        );

        function getSpecialAuthorityRequestState(
            hdid: string
        ): SpecialAuthorityRequestState {
            return datasetMapUtil.getDatasetState(
                medicationRequestStateMap.value,
                hdid
            );
        }

        function specialAuthorityRequests(hdid: string): MedicationRequest[] {
            return getSpecialAuthorityRequestState(hdid).data;
        }

        function specialAuthorityRequestsCount(hdid: string): number {
            return getSpecialAuthorityRequestState(hdid).data.length;
        }

        function specialAuthorityRequestsAreLoading(hdid: string): boolean {
            return (
                getSpecialAuthorityRequestState(hdid).status ===
                LoadStatus.REQUESTED
            );
        }

        function setSpecialAuthorityRequests(
            hdid: string,
            requestResults: RequestResult<MedicationRequest[]>
        ) {
            if (requestResults.resultStatus === ResultType.Success) {
                datasetMapUtil.setStateData(
                    medicationRequestStateMap.value,
                    hdid,
                    requestResults.resourcePayload
                );
            } else {
                datasetMapUtil.setStateError(
                    medicationRequestStateMap.value,
                    hdid,
                    requestResults.resultError
                );
            }
        }

        function handleError(
            hdid: string,
            error: ResultError,
            errorType: ErrorType
        ) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);

            datasetMapUtil.setStateError(
                medicationRequestStateMap.value,
                hdid,
                error
            );
            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    errorType,
                    ErrorSourceType.MedicationRequests,
                    error.traceId
                );
            }
        }

        function retrieveSpecialAuthorityRequests(
            hdid: string
        ): Promise<RequestResult<MedicationRequest[]>> {
            if (
                getSpecialAuthorityRequestState(hdid).status ===
                LoadStatus.LOADED
            ) {
                logger.debug(
                    "Special Authority requests found stored, not querying!"
                );
                return Promise.resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: specialAuthorityRequests(hdid),
                    resultStatus: ResultType.Success,
                    totalResultCount: specialAuthorityRequestsCount(hdid),
                });
            }

            datasetMapUtil.setStateRequested(
                medicationRequestStateMap.value,
                hdid
            );
            return specialAuthorityService
                .getPatientMedicationRequest(hdid)
                .then((result) => {
                    if (result.resultStatus === ResultType.Error) {
                        throw result.resultError;
                    }
                    EventTracker.loadData(
                        EntryType.SpecialAuthorityRequest,
                        result.resourcePayload.length
                    );
                    setSpecialAuthorityRequests(hdid, result);
                    return result;
                })
                .catch((error: ResultError) => {
                    handleError(hdid, error, ErrorType.Retrieve);
                    throw error;
                });
        }

        return {
            specialAuthorityRequests,
            specialAuthorityRequestsCount,
            specialAuthorityRequestsAreLoading,
            retrieveSpecialAuthorityRequests,
        };
    }
);
