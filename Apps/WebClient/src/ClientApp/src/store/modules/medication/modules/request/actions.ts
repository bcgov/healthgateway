import { ResultType } from "@/constants/resulttype";
import MedicationRequest from "@/models/MedicationRequest";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IMedicationService } from "@/services/interfaces";

import { MedicationRequestActions } from "./types";

export const actions: MedicationRequestActions = {
    retrieveMedicationRequests(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<MedicationRequest[]>> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const medicationService: IMedicationService =
            container.get<IMedicationService>(
                SERVICE_IDENTIFIER.MedicationService
            );

        return new Promise((resolve, reject) => {
            const medicationRequests: MedicationRequest[] =
                context.getters.medicationRequests;
            if (
                context.state.status === LoadStatus.LOADED ||
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
                context.commit("setMedicationRequestRequested");
                return medicationService
                    .getPatientMedicationRequest(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch(
                                "handleRequestError",
                                result.resultError
                            );
                            reject(result.resultError);
                        } else {
                            context.commit(
                                "setMedicationRequestResult",
                                result
                            );
                            resolve(result);
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleRequestError", error);
                        reject(error);
                    });
            }
        });
    },
    handleRequestError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("medicationRequestError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Medication Requests Error", error },
            { root: true }
        );
    },
};
