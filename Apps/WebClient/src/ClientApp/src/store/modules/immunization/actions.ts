import { ActionTree, Commit } from "vuex";

import { ResultType } from "@/constants/resulttype";
import ImmunizationModel from "@/models/immunizationModel";
import { ImmunizationState, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IImmunizationService, ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`ERROR: ${JSON.stringify(error)}`);
    commit("immunizationError");
}

const immunizationService: IImmunizationService = container.get<IImmunizationService>(
    SERVICE_IDENTIFIER.ImmunizationService
);

export const actions: ActionTree<ImmunizationState, RootState> = {
    retrieve(context, params: { hdid: string }): Promise<ImmunizationModel[]> {
        return new Promise((resolve, reject) => {
            const immunizationModels: ImmunizationModel[] =
                context.getters.getStoredImmunizations;
            if (
                immunizationModels.length > 0 &&
                !context.getters.isDeferredLoad
            ) {
                logger.debug(`Immunizations found stored, not quering!`);
                resolve([]);
            } else {
                logger.debug(`Retrieving Immunizations`);
                immunizationService
                    .getPatientImmunizations(params.hdid)
                    .then((immunizationRequestResult) => {
                        if (
                            immunizationRequestResult.resultStatus ===
                            ResultType.Success
                        ) {
                            const results =
                                immunizationRequestResult.resourcePayload;
                            if (results.loadState.refreshInProgress) {
                                logger.info("Immunizations load deferred");
                                setTimeout(() => {
                                    logger.info("Re-quering for immunizations");
                                    context.dispatch("retrieve", {
                                        hdid: params.hdid,
                                    });
                                }, 10000);
                            }
                            context.commit("setImmunizationResult", results);
                            resolve([]);
                        } else {
                            reject(immunizationRequestResult.resultError);
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
