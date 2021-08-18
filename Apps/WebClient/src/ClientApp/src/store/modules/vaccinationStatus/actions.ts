import { ResultType } from "@/constants/resulttype";
import { StringISODate } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";

import { VaccinationStatusActions } from "./types";

export const actions: VaccinationStatusActions = {
    retrieve(
        context,
        params: { phn: string; dateOfBirth: StringISODate }
    ): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService: IVaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        return new Promise((resolve, reject) => {
            logger.debug(`Retrieving Vaccination Status`);
            context.commit("setRequested");
            vaccinationStatusService
                .getVaccinationStatus(params.phn, params.dateOfBirth)
                .then((result) => {
                    if (result.resultStatus === ResultType.Success) {
                        const payload = result.resourcePayload;
                        if (!payload.loaded && payload.retryin > 0) {
                            logger.info("VaccinationStatus not loaded");
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for vaccination status"
                                );
                                context.dispatch("retrieve", {
                                    phn: params.phn,
                                    dateOfBirth: params.dateOfBirth,
                                });
                            }, payload.retryin);
                        }

                        context.commit("setVaccinationStatus", payload);
                        resolve();
                    } else {
                        context.dispatch("handleError", result.resultError);
                        reject(result.resultError);
                    }
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    handleError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("vaccinationStatusError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Vaccination Status Error", error },
            { root: true }
        );
    },
};
