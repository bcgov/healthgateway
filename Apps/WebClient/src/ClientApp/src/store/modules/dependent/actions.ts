import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IDependentService, ILogger } from "@/services/interfaces";

import { DependentActions } from "./types";

export const actions: DependentActions = {
    retrieveDependents(
        context,
        params: { hdid: string; bypassCache: boolean }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );

        return new Promise((resolve, reject) => {
            if (
                context.state.status === LoadStatus.LOADED &&
                !params.bypassCache
            ) {
                logger.debug("Dependents found stored, not querying!");
                resolve();
            } else {
                logger.debug("Retrieving dependents");
                context.commit("setDependentsRequested");
                dependentService
                    .getAll(params.hdid)
                    .then((result) => {
                        result.sort((a, b) => {
                            const firstDate = new DateWrapper(
                                a.dependentInformation.dateOfBirth
                            );
                            const secondDate = new DateWrapper(
                                b.dependentInformation.dateOfBirth
                            );

                            if (firstDate.isBefore(secondDate)) {
                                return 1;
                            }

                            if (firstDate.isAfter(secondDate)) {
                                return -1;
                            }

                            return 0;
                        });
                        context.commit("setDependents", result);
                        resolve();
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleDependentsError", {
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleDependentsError(
        context,
        params: { error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("setDependentsError", params.error);

        if (params.error.statusCode === 429) {
            let action = "errorBanner/setTooManyRequestsError";
            if (params.errorType === ErrorType.Retrieve) {
                action = "errorBanner/setTooManyRequestsWarning";
            }
            context.dispatch(action, { key: "page" }, { root: true });
        } else {
            context.dispatch(
                "errorBanner/addError",
                {
                    errorType: params.errorType,
                    source: ErrorSourceType.Dependent,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
