import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus, MedicationState, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger, IMedicationService } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const medicationService: IMedicationService = container.get<IMedicationService>(
    SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<MedicationState, RootState> = {
    retrieve(
        context,
        params: { hdid: string; protectiveWord?: string }
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        return new Promise((resolve, reject) => {
            const medicationStatements: MedicationStatementHistory[] =
                context.getters.medicationStatements;
            if (context.state.status === LoadStatus.LOADED) {
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
                context.commit("setRequested");
                return medicationService
                    .getPatientMedicationStatementHistory(
                        params.hdid,
                        params.protectiveWord
                    )
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", result.resultError);
                            reject(result.resultError);
                        } else {
                            context.commit("setMedicationResult", result);
                            resolve(result);
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleError", error);
                        reject(error);
                    });
            }
        });
    },
    handleError(context, error: ResultError) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("medicationError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Medications Error", error },
            { root: true }
        );
    },
};
