import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { HttpError } from "@/models/errors";
import Patient from "@/models/patient";
import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IPatientDataService } from "@/services/interfaces";
import { PatientDataActions } from "@/store/modules/patientData/types";
import { getPatientDataRecordState } from "@/store/modules/patientData/utils";
import EventTracker from "@/utility/eventTracker";

export const actions: PatientDataActions = {
    retrievePatientData(
        context,
        params: { hdid: string }
    ): Promise<PatientData> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const patientDataService = container.get<IPatientDataService>(
            SERVICE_IDENTIFIER.PatientDataService
        );

        return new Promise((resolve, reject) => {
            if (
                getPatientDataRecordState(context.state, params.hdid).status ===
                LoadStatus.LOADED
            ) {
                logger.debug("Patient data found stored, not querying!");
                const patientData: PatientData = context.getters.patientData(
                    params.hdid
                );
                resolve(patientData);
            } else {
                logger.debug("Retrieving patient data");
                context.commit("setPatientDataRequested", params.hdid);
                patientDataService
                    .getPatientData(params.hdid)
                    .then((data: PatientData | undefined) => {
                        if (data === undefined) {
                            reject(new Error("No patient data was returned"));
                            return;
                        }
                        context.commit("setPatientData", {
                            hdid: params.hdid,
                            patientData: data,
                        });
                        resolve(data);
                    })
                    .catch((error: HttpError) => {
                        context.dispatch("handleError", {
                            error,
                            errorType: ErrorType.Retrieve,
                            hdid: params.hdid,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleError(
        context,
        params: {
            error: HttpError;
            errorType: ErrorType;
            hdid?: string;
            fileId: string;
        }
    ): void {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        if (params.fileId) {
            context.commit("setFileError", {
                fileId: params.fileId,
                error: params.error,
            });
        } else {
            context.commit("setPatientDataError", {
                hdid: params.hdid,
                error: params.error,
            });
        }

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
                    source: ErrorSourceType.ClinicalDocument,
                    traceId: "", // TODO: Validate this approach
                },
                { root: true }
            );
        }
    },
};
