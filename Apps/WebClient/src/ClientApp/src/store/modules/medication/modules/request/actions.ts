import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/errors";
import MedicationRequest from "@/models/medicationRequest";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, ISpecialAuthorityService } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { MedicationRequestActions } from "./types";
import { getSpecialAuthorityRequestState } from "./util";

export const actions: MedicationRequestActions = {
    retrieveMedicationRequests(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<MedicationRequest[]>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const specialAuthorityService = container.get<ISpecialAuthorityService>(
            SERVICE_IDENTIFIER.SpecialAuthorityService
        );

        return new Promise((resolve, reject) => {
            const medicationRequests: MedicationRequest[] =
                context.getters.medicationRequests(params.hdid);
            if (
                getSpecialAuthorityRequestState(context.state, params.hdid)
                    .status === LoadStatus.LOADED ||
                medicationRequests.length > 0
            ) {
                logger.debug("Medication Requests found stored, not querying!");
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: medicationRequests,
                    resultStatus: ResultType.Success,
                    totalResultCount: medicationRequests.length,
                });
            } else {
                logger.debug("Retrieving Medication Requests");
                context.commit("setMedicationRequestRequested", params.hdid);
                return specialAuthorityService
                    .getPatientMedicationRequest(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleMedicationRequestError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        } else {
                            EventTracker.loadData(
                                EntryType.MedicationRequest,
                                result.resourcePayload.length
                            );
                            context.commit("setMedicationRequestResult", {
                                hdid: params.hdid,
                                medicationRequestResult: result,
                            });
                            resolve(result);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleMedicationRequestError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleMedicationRequestError(
        context,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("medicationRequestError", {
            hdid: params.hdid,
            error: params.error,
        });

        if (params.error.statusCode === 429) {
            context.dispatch(
                "errorBanner/setTooManyRequestsWarning",
                { key: "page" },
                { root: true }
            );
        } else {
            context.dispatch(
                "errorBanner/addError",
                {
                    errorType: params.errorType,
                    source: ErrorSourceType.MedicationRequests,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
