import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/requestResult";
import { ImmunizationState, LoadStatus, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IImmunizationService, ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const immunizationService: IImmunizationService = container.get<IImmunizationService>(
    SERVICE_IDENTIFIER.ImmunizationService
);

export const actions: ActionTree<ImmunizationState, RootState> = {
    retrieve(context, params: { hdid: string }): Promise<void> {
        return new Promise((resolve, reject) => {
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Immunizations found stored, not quering!`);
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
                                    logger.info("Re-quering for immunizations");
                                    context.dispatch("retrieve", {
                                        hdid: params.hdid,
                                    });
                                }, 10000);
                            }
                            context.commit("setImmunizationResult", payload);
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
            }
        });
    },
    handleError(context, error: ResultError) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("immunizationError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Immunizations Error", error },
            { root: true }
        );
    },
};
