import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult, { ResultError } from "@/models/requestResult";
import {
    LoadStatus,
    MedicationStatementState,
    RootState,
} from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger, IMedicationService } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const medicationService: IMedicationService = container.get<IMedicationService>(
    SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<MedicationStatementState, RootState> = {
    retrieveMedicationStatements(
        context,
        params: { hdid: string; protectiveWord?: string }
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        return new Promise((resolve, reject) => {
            const medicationStatements: MedicationStatementHistory[] =
                context.getters.medicationStatements;
            if (
                context.state.status === LoadStatus.LOADED ||
                medicationStatements.length > 0
            ) {
                logger.debug(
                    "Medication Statements found stored, not quering!"
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
                context.commit("setMedicationStatementRequested");
                return medicationService
                    .getPatientMedicationStatementHistory(
                        params.hdid,
                        params.protectiveWord
                    )
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch(
                                "handleStatementError",
                                result.resultError
                            );
                            reject(result.resultError);
                        } else {
                            context.commit(
                                "setMedicationStatementResult",
                                result
                            );
                            resolve(result);
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleStatementError", error);
                        reject(error);
                    });
            }
        });
    },
    handleStatementError(context, error: ResultError) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("medicationStatementError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Medication Statements Error", error },
            { root: true }
        );
    },
};
