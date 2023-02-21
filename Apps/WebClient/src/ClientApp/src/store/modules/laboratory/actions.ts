import { ActionType } from "@/constants/actionType";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { StringISODate } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryOrderResult,
    LaboratoryOrder,
    LaboratoryOrderResult,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { LaboratoryActions } from "./types";
import { getCovid19TestResultState, getLabResultState } from "./util";

export const actions: LaboratoryActions = {
    retrieveCovid19LaboratoryOrders(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );

        return new Promise((resolve, reject) => {
            if (
                getCovid19TestResultState(context.state, params.hdid).status ===
                LoadStatus.LOADED
            ) {
                logger.debug(
                    "COVID-19 Laboratory Orders found stored, not querying!"
                );
                const covid19LaboratoryOrders: Covid19LaboratoryOrder[] =
                    context.getters.covid19LaboratoryOrders(params.hdid);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        retryin: 0,
                        orders: covid19LaboratoryOrders,
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: covid19LaboratoryOrders.length,
                });
            } else {
                logger.debug("Retrieving COVID-19 Laboratory Orders");
                context.commit(
                    "setCovid19LaboratoryOrdersRequested",
                    params.hdid
                );
                laboratoryService
                    .getCovid19LaboratoryOrders(params.hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (result.resultStatus === ResultType.Success) {
                            EventTracker.loadData(
                                EntryType.Covid19TestResult,
                                result.totalResultCount
                            );
                            context.commit("setCovid19LaboratoryOrders", {
                                hdid: params.hdid,
                                laboratoryOrders: payload.orders,
                            });
                            resolve(result);
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info(
                                "COVID-19 Laboratory Orders not loaded"
                            );
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for COVID-19 Laboratory Orders"
                                );
                                context.dispatch(
                                    "retrieveCovid19LaboratoryOrders",
                                    {
                                        hdid: params.hdid,
                                    }
                                );
                            }, payload.retryin);
                            resolve(result);
                        } else {
                            context.dispatch("handleError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                                errorSourceType:
                                    ErrorSourceType.Covid19Laboratory,
                            });
                            reject(result.resultError);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                            errorSourceType: ErrorSourceType.Covid19Laboratory,
                        });
                        reject(error);
                    });
            }
        });
    },
    retrieveLaboratoryOrders(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrderResult>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );

        return new Promise((resolve, reject) => {
            if (
                getLabResultState(context.state, params.hdid).status ===
                LoadStatus.LOADED
            ) {
                logger.debug("Laboratory Orders found stored, not querying!");
                const laboratoryOrders: LaboratoryOrder[] =
                    context.getters.laboratoryOrders(params.hdid);
                const laboratoryQueued: boolean =
                    context.getters.laboratoryOrdersAreQueued(params.hdid);
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        queued: laboratoryQueued,
                        retryin: 0,
                        orders: laboratoryOrders,
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: laboratoryOrders.length,
                });
            } else {
                logger.debug("Retrieving Laboratory Orders");
                context.commit("setLaboratoryOrdersRequested", params.hdid);
                laboratoryService
                    .getLaboratoryOrders(params.hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (
                            result.resultStatus === ResultType.Success &&
                            payload.loaded
                        ) {
                            EventTracker.loadData(
                                EntryType.LabResult,
                                result.totalResultCount
                            );
                            logger.info("Laboratory Orders loaded.");
                            context.commit("setLaboratoryOrders", {
                                hdid: params.hdid,
                                laboratoryOrderResult: payload,
                            });
                            resolve(result);
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info(
                                "Refresh in progress... partially load Laboratory Orders"
                            );
                            context.commit(
                                "setLaboratoryOrdersRefreshInProgress",
                                {
                                    hdid: params.hdid,
                                    laboratoryOrderResult: payload,
                                }
                            );
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for Laboratory Orders"
                                );
                                context.dispatch("retrieveLaboratoryOrders", {
                                    hdid: params.hdid,
                                });
                            }, payload.retryin);
                            resolve(result);
                        } else {
                            context.dispatch("handleError", {
                                hdid: params.hdid,
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                                errorSourceType: ErrorSourceType.Laboratory,
                            });
                            reject(result.resultError);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            hdid: params.hdid,
                            error,
                            errorType: ErrorType.Retrieve,
                            errorSourceType: ErrorSourceType.Laboratory,
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
            case ErrorSourceType.Laboratory:
                context.commit("setLaboratoryError", {
                    hdid: params.hdid,
                    error: params.error,
                });
                break;
            case ErrorSourceType.Covid19Laboratory:
                context.commit("setCovid19LaboratoryError", {
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
    retrievePublicCovidTests(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            collectionDate: StringISODate;
        }
    ): Promise<PublicCovidTestResponseResult> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );

        return new Promise((resolve, reject) => {
            logger.debug(`Retrieving Public Covid Tests in store.`);
            context.commit("setPublicCovidTestResponseResultRequested");
            laboratoryService
                .getPublicCovid19Tests(
                    params.phn,
                    params.dateOfBirth,
                    params.collectionDate
                )
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        context.commit(
                            "setPublicCovidTestResponseResult",
                            payload
                        );
                        resolve(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Public Covid Tests not loaded in store.");
                        context.commit(
                            "setPublicCovidTestResponseResultStatusMessage",
                            "We're busy but will continue to try to find the Public Covid Tests...."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for finding Public Covid Tests in store."
                            );
                            context.dispatch("retrievePublicCovidTests", {
                                phn: params.phn,
                                dateOfBirth: params.dateOfBirth,
                                collectionDate: params.collectionDate,
                            });
                        }, payload.retryin);
                        resolve(payload);
                    } else {
                        context.dispatch("handlePublicCovidTestsError", {
                            error: result.resultError,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(result.resultError);
                    }
                })
                .catch((error: ResultError) => {
                    context.dispatch("handlePublicCovidTestsError", {
                        error,
                        errorType: ErrorType.Retrieve,
                    });
                    reject(error);
                });
        });
    },
    handlePublicCovidTestsError(
        context,
        params: {
            error: ResultError;
            errorType: ErrorType;
        }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);

        if (params.error.statusCode === 429) {
            context.dispatch(
                "errorBanner/setTooManyRequestsWarning",
                { key: "publicCovidTest" },
                { root: true }
            );

            context.commit("setPublicCovidTestResponseResultError", undefined);
        } else {
            const customBannerError: CustomBannerError = {
                title: "Our Apologies",
                description:
                    "We've found an issue and the Health Gateway team is working hard to fix it.",
            };

            if (params.error.actionCode === ActionType.DataMismatch) {
                customBannerError.title = "Data Mismatch";
                customBannerError.description =
                    "The information you entered does not match our records. Please try again.";
                customBannerError.detail =
                    "Please note that it can take up to 48 hours from the time of test before a result is available. If it has been at least 48 hours since you tested, please contact the COVID‑19 Results Line (1‐833‐707‐2792) to investigate the issue.";
            }

            context.commit(
                "setPublicCovidTestResponseResultError",
                customBannerError
            );
        }
    },
    resetPublicCovidTestResponseResult(context) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.debug(
            `Resetting Laboratory store module for Public COVID-19 Test response result.`
        );
        context.commit("resetPublicCovidTestResponseResult");
    },
};
