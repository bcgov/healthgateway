import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IImmunizationService, ILogger } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { ImmunizationActions } from "./types";
import { getImmunizationDatasetState } from "./util";

export const actions: ImmunizationActions = {
    retrieve(context, params: { hdid: string }): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const immunizationService = container.get<IImmunizationService>(
            SERVICE_IDENTIFIER.ImmunizationService
        );

        return new Promise((resolve, reject) => {
            if (
                getImmunizationDatasetState(context.state, params.hdid)
                    .status === LoadStatus.LOADED
            ) {
                logger.debug(`Immunizations found stored, not querying!`);
                resolve();
            } else {
                logger.debug(`Retrieving Immunizations`);
                context.commit("setImmunizationRequested", params.hdid);
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

                            context.commit("setImmunizationResult", {
                                hdid: params.hdid,
                                immunizationResult: payload,
                            });
                            resolve();
                        } else {
                            context.dispatch("handleError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleError(
        context,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("immunizationError", {
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
                    source: ErrorSourceType.Immunization,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
