import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import PatientData from "@/models/patientData";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus, PatientState, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger, IPatientService } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const patientService: IPatientService = container.get<IPatientService>(
    SERVICE_IDENTIFIER.PatientService
);

export const actions: ActionTree<PatientState, RootState> = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<PatientData>> {
        return new Promise((resolve, reject) => {
            const patientData: PatientData = context.getters.patientData;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Patient Data found stored, not quering!`);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: patientData,
                    resultStatus: ResultType.Success,
                    totalResultCount: 1,
                });
            } else {
                logger.debug(`Retrieving Patient Data`);
                context.commit("setRequested");
                patientService
                    .getPatientData(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", result.resultError);
                            reject(result.resultError);
                        } else {
                            context.commit(
                                "setPatientData",
                                result.resourcePayload
                            );
                            resolve(result);
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
        context.commit("patientError", error);
        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Patient Data Error", error },
            { root: true }
        );
    },
};
