import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IMedicationService } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { MedicationStatementActions } from "./types";
import { getMedicationState } from "./util";

export const actions: MedicationStatementActions = {
    retrieveMedicationStatements(
        context,
        params: { hdid: string; protectiveWord?: string }
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const medicationService = container.get<IMedicationService>(
            SERVICE_IDENTIFIER.MedicationService
        );

        return new Promise((resolve, reject) => {
            const medicationStatements: MedicationStatementHistory[] =
                context.getters.medicationStatements(params.hdid);
            if (
                getMedicationState(context.state, params.hdid).status ===
                    LoadStatus.LOADED ||
                medicationStatements.length > 0
            ) {
                logger.debug(
                    "Medication Statements found stored, not querying!"
                );
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: medicationStatements,
                    resultStatus: ResultType.Success,
                    totalResultCount: medicationStatements.length,
                });
            } else {
                logger.debug("Retrieving Medication Statements");
                context.commit("setMedicationStatementRequested", params.hdid);
                return medicationService
                    .getPatientMedicationStatementHistory(
                        params.hdid,
                        params.protectiveWord
                    )
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleMedicationStatementError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        } else {
                            if (result.resultStatus === ResultType.Success) {
                                EventTracker.loadData(
                                    EntryType.Medication,
                                    result.resourcePayload.length
                                );
                            }
                            context.commit("setMedicationStatementResult", {
                                hdid: params.hdid,
                                medicationResult: result,
                            });
                            resolve(result);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleMedicationStatementError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleMedicationStatementError(
        context,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("medicationStatementError", {
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
                    source: ErrorSourceType.MedicationStatements,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
