import { ActionTree, Commit } from "vuex";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { LaboratoryState, RootState } from "@/models/storeState";
import { ILogger, ILaboratoryService } from "@/services/interfaces";
import { LaboratoryOrder } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`ERROR: ${JSON.stringify(error)}`);
    commit("laboratoryError");
}

const laboratoryService: ILaboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);

export const actions: ActionTree<LaboratoryState, RootState> = {
    getOrders(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise((resolve, reject) => {
            const laboratoryOrders: LaboratoryOrder[] = context.getters.getStoredLaboratoryOrders();
            if (laboratoryOrders.length > 0) {
                logger.debug(`Laboratory found stored, not quering!`);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: laboratoryOrders,
                    resultMessage: "From storage",
                    resultStatus: ResultType.Success,
                    totalResultCount: laboratoryOrders.length,
                    errorCode: "",
                });
            } else {
                logger.debug(`Retrieving Laboratory Orders`);
                laboratoryService
                    .getOrders(params.hdid)
                    .then((laboratoryOrders) => {
                        context.commit("setLaboratoryOrders", laboratoryOrders);
                        resolve(laboratoryOrders);
                    })
                    .catch((error) => {
                        handleError(context.commit, error);
                        reject(error);
                    });
            }
        });
    },
};
