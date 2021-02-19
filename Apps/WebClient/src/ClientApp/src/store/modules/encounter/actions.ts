import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import Encounter from "@/models/encounter";
import RequestResult from "@/models/requestResult";
import { EncounterState, LoadStatus, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IEncounterService, ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const encounterService: IEncounterService = container.get<IEncounterService>(
    SERVICE_IDENTIFIER.EncounterService
);

export const actions: ActionTree<EncounterState, RootState> = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<Encounter[]>> {
        return new Promise((resolve, reject) => {
            const patientEncounters: Encounter[] =
                context.getters.patientEncounters;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Encounters found stored, not quering!`);
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
                        context.commit(
                            "setPatientEncounters",
                            result.resourcePayload
                        );
                        resolve(result);
                    })
                    .catch((error) => {
                        logger.error(`ERROR: ${JSON.stringify(error)}`);
                        context.commit("encounterError", error);
                        reject(error);
                    });
            }
        });
    },
};
