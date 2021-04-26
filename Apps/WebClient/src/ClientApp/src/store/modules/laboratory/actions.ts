import { ResultType } from "@/constants/resulttype";
import { LaboratoryOrder } from "@/models/laboratory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILaboratoryService, ILogger } from "@/services/interfaces";

import { LaboratoryActions } from "./types";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const laboratoryService: ILaboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);

export const actions: LaboratoryActions = {
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
                        if (result.resultStatus === ResultType.Success) {
                            context.commit(
                                "setLaboratoryOrders",
                                result.resourcePayload
                            );
                            resolve(result);
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
        context.commit("laboratoryError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Laboratory Orders Error", error },
            { root: true }
        );
    },
};
