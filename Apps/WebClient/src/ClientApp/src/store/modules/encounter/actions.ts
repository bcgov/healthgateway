import { ActionType } from "@/constants/actionType";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { Encounter, HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IEncounterService,
    IHospitalVisitService,
    ILogger,
} from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { EncounterActions } from "./types";

export const actions: EncounterActions = {
    retrievePatientEncounters(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<Encounter[]>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const encounterService = container.get<IEncounterService>(
            SERVICE_IDENTIFIER.EncounterService
        );

        return new Promise((resolve, reject) => {
            if (
                context.state.encounters[params.hdid]?.status ===
                LoadStatus.LOADED
            ) {
                logger.debug(`Encounters found stored, not querying!`);
                const patientEncounters: Encounter[] =
                    context.getters.patientEncounters(params.hdid);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: patientEncounters,
                    resultStatus: ResultType.Success,
                    totalResultCount: patientEncounters.length,
                });
            } else {
                logger.debug(`Retrieving Patient Encounters`);
                context.commit("setPatientEncountersRequested", params.hdid);
                encounterService
                    .getPatientEncounters(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                                errorSourceType: ErrorSourceType.Encounter,
                            });
                            reject(result.resultError);
                        } else {
                            EventTracker.loadData(
                                EntryType.Encounter,
                                result.resourcePayload.length
                            );
                            context.commit("setPatientEncounters", {
                                hdid: params.hdid,
                                patientEncounters: result.resourcePayload,
                            });
                            resolve(result);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                            errorSourceType: ErrorSourceType.Encounter,
                        });
                        reject(error);
                    });
            }
        });
    },
    retrieveHospitalVisits(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<HospitalVisitResult>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const hospitalVisitService = container.get<IHospitalVisitService>(
            SERVICE_IDENTIFIER.HospitalVisitService
        );

        return new Promise((resolve, reject) => {
            if (
                context.state.hospitalVisits[params.hdid]?.status ===
                LoadStatus.LOADED
            ) {
                logger.debug(`Hospital Visits found stored, not querying!`);
                const hospitalVisits: HospitalVisit[] =
                    context.getters.hospitalVisits(params.hdid);
                const hospitalVisitsQueued: boolean =
                    context.getters.isHospitalVisitQueued(params.hdid);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        queued: hospitalVisitsQueued,
                        retryin: 0,
                        hospitalVisits: hospitalVisits,
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: hospitalVisits.length,
                });
            } else {
                logger.debug(`Retrieving Hospital Visits`);
                context.commit("setHospitalVisitsRequested", params.hdid);
                hospitalVisitService
                    .getHospitalVisits(params.hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (
                            result.resultStatus === ResultType.Success &&
                            payload.loaded
                        ) {
                            EventTracker.loadData(
                                EntryType.HospitalVisit,
                                result.totalResultCount
                            );
                            logger.info("Hospital Visits loaded.");
                            context.commit("setHospitalVisits", {
                                hdid: params.hdid,
                                hospitalVisitResult: payload,
                            });
                            resolve(result);
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info(
                                "Refresh in progress... partially load Hospital Visits"
                            );
                            context.commit(
                                "setHospitalVisitRefreshInProgress",
                                { hdid: params.hdid, hospitalVisits: payload }
                            );
                            setTimeout(() => {
                                logger.info("Re-querying for Hospital Visits");
                                context.dispatch("retrieveHospitalVisits", {
                                    hdid: params.hdid,
                                });
                            }, payload.retryin);
                            resolve(result);
                        } else {
                            context.dispatch("handleError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                                errorSourceType: ErrorSourceType.HospitalVisit,
                            });
                            reject(result.resultError);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                            errorSourceType: ErrorSourceType.HospitalVisit,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleError(
        context,
        params: {
            hdid: string;
            error: ResultError;
            errorType: ErrorType;
            errorSourceType: ErrorSourceType;
        }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);

        switch (params.errorSourceType) {
            case ErrorSourceType.Encounter:
                context.commit("encounterError", {
                    hdid: params.hdid,
                    error: params.error,
                });
                break;
            case ErrorSourceType.HospitalVisit:
                context.commit("hospitalVisitsError", {
                    hdid: params.hdid,
                    error: params.error,
                });
                break;
            default:
                break;
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
                    source: params.errorSourceType,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
