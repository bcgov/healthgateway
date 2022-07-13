import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import Encounter from "@/models/encounter";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IEncounterService, ILogger } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { EncounterActions } from "./types";

export const actions: EncounterActions = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<Encounter[]>> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const encounterService: IEncounterService =
            container.get<IEncounterService>(
                SERVICE_IDENTIFIER.EncounterService
            );

        return new Promise((resolve, reject) => {
            const patientEncounters: Encounter[] =
                context.getters.patientEncounters;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Encounters found stored, not querying!`);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: patientEncounters,
                    resultStatus: ResultType.Success,
                    totalResultCount: patientEncounters.length,
                });
            } else {
                logger.debug(`Retrieving Patient Encounters`);
                context.commit("setRequested");
                encounterService
                    .getPatientEncounters(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", {
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        } else {
                            EventTracker.loadData(
                                EntryType.Encounter,
                                result.resourcePayload.length
                            );
                            context.commit(
                                "setPatientEncounters",
                                result.resourcePayload
                            );
                            resolve(result);
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
        context.commit("encounterError", params.error);

        if (
            params.errorType === ErrorType.Retrieve &&
            params.error.statusCode === 429
        ) {
            context.dispatch(
                "errorBanner/setTooManyRequestsWarning",
                {
                    key: "page",
                },
                {
                    root: true,
                }
            );
        } else {
            context.dispatch(
                "errorBanner/addError",
                {
                    errorType: params.errorType,
                    source: ErrorSourceType.Encounter,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
