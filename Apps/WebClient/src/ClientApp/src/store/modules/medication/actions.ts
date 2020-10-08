import { ActionTree, Commit } from "vuex";
import { ILogger, IMedicationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { MedicationState, RootState } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`ERROR: ${error}`);
    commit("medicationError");
}

const medicationService: IMedicationService = container.get<IMedicationService>(
    SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<MedicationState, RootState> = {
    getMedicationStatements(
        context,
        params: { hdid: string; protectiveWord?: string }
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        return new Promise((resolve, reject) => {
            const medicationStatements: MedicationStatementHistory[] = context.getters.getStoredMedicationStatements();
            if (medicationStatements.length > 0) {
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
                medicationService
                    .getPatientMedicationStatementHistory(
                        params.hdid,
                        params.protectiveWord
                    )
                    .then((result) => {
                        if (result.resultStatus === ResultType.Success) {
                            context.commit(
                                "setMedicationStatements",
                                result.resourcePayload
                            );
                        }
                        resolve(result);
                    })
                    .catch((error) => {
                        handleError(context.commit, error);
                        reject(error);
                    });
            }
        });
    },
    getMedicationInformation(
        context,
        params: { din: string }
    ): Promise<MedicationResult> {
        return new Promise((resolve, reject) => {
            const medicationResult = context.getters.getStoredMedicationInformation(
                params.din
            );
            if (medicationResult) {
                logger.debug("Medication found stored, not quering!");
                resolve(medicationResult);
            } else {
                logger.debug("Retrieving Medication info...");
                medicationService
                    .getMedicationInformation(params.din)
                    .then((medicationData) => {
                        if (medicationData) {
                            context.commit(
                                "addMedicationInformation",
                                medicationData
                            );
                            resolve(medicationData);
                        } else {
                            resolve(undefined);
                        }
                    })
                    .catch((error) => {
                        handleError(context.commit, error);
                        reject(error);
                    });
            }
        });
    },
};
