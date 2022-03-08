import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IImmunizationService, ILogger } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { ImmunizationActions } from "./types";

export const actions: ImmunizationActions = {
    retrieve(context, params: { hdid: string }): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const immunizationService: IImmunizationService =
            container.get<IImmunizationService>(
                SERVICE_IDENTIFIER.ImmunizationService
            );

        return new Promise((resolve, reject) => {
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Immunizations found stored, not querying!`);
                resolve();
            } else {
                logger.debug(`Retrieving Immunizations`);
                context.commit("setRequested");
                immunizationService
                    .getPatientImmunizations(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Success) {
                            const payload = result.resourcePayload;
                            if (payload.loadState.refreshInProgress) {
                                logger.info("Immunizations load deferred");
                                setTimeout(() => {
                                    logger.info(
                                        "Re-querying for immunizations"
                                    );
                                    context.dispatch("retrieve", {
                                        hdid: params.hdid,
                                    });
                                }, 10000);
                            } else {
                                EventTracker.loadData(
                                    EntryType.Immunization,
                                    result.resourcePayload.immunizations.length
                                );
                            }

                            context.commit("setImmunizationResult", payload);
                            resolve();
                        } else {
                            context.dispatch("handleError", {
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleError(context, params: { error: ResultError; errorType: ErrorType }) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("immunizationError", params.error);

        context.dispatch(
            "errorBanner/addError",
            {
                errorType: params.errorType,
                source: ErrorSourceType.Immunization,
                traceId: params.error.traceId,
            },
            { root: true }
        );
    },
};
