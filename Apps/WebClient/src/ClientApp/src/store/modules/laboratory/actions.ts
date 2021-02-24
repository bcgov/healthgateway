import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import { LaboratoryOrder } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { LaboratoryState, LoadStatus, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILaboratoryService, ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const laboratoryService: ILaboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);

export const actions: ActionTree<LaboratoryState, RootState> = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise((resolve, reject) => {
            const laboratoryOrders: LaboratoryOrder[] =
                context.getters.laboratoryOrders;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Laboratory found stored, not quering!`);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: laboratoryOrders,
                    resultStatus: ResultType.Success,
                    totalResultCount: laboratoryOrders.length,
                });
            } else {
                logger.debug(`Retrieving Laboratory Orders`);
                context.commit("setRequested");
                laboratoryService
                    .getOrders(params.hdid)
                    .then((result) => {
                        context.commit(
                            "setLaboratoryOrders",
                            result.resourcePayload
                        );
                        resolve(result);
                    })
                    .catch((error) => {
                        logger.error(`ERROR: ${JSON.stringify(error)}`);
                        context.commit("laboratoryError", error);
                        reject(error);
                    });
            }
        });
    },
};
