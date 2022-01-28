import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import { StringISODate } from "@/models/dateWrapper";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryOrderResult,
    LaboratoryOrder,
    LaboratoryOrderResult,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { EntryType } from "@/models/timelineEntry";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { LaboratoryActions } from "./types";

export const actions: LaboratoryActions = {
    retrieveCovid19LaboratoryOrders(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const laboratoryService: ILaboratoryService =
            container.get<ILaboratoryService>(
                SERVICE_IDENTIFIER.LaboratoryService
            );

        return new Promise((resolve, reject) => {
            const covid19LaboratoryOrders: Covid19LaboratoryOrder[] =
                context.getters.covid19LaboratoryOrders;
            if (
                context.state.authenticatedCovid19.status === LoadStatus.LOADED
            ) {
                logger.debug(
                    "COVID-19 Laboratory Orders found stored, not querying!"
                );
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
                context.commit("setCovid19LaboratoryOrdersRequested");
                laboratoryService
                    .getCovid19LaboratoryOrders(params.hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (result.resultStatus === ResultType.Success) {
                            EventTracker.loadData(
                                EntryType.Covid19LaboratoryOrder,
                                result.totalResultCount
                            );
                            context.commit(
                                "setCovid19LaboratoryOrders",
                                payload.orders
                            );
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
                            context.dispatch(
                                "handleCovid19LaboratoryError",
                                result.resultError
                            );
                            reject(result.resultError);
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleCovid19LaboratoryError", error);
                        reject(error);
                    });
            }
        });
    },
    handleCovid19LaboratoryError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("covid19LaboratoryError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch COVID-19 Laboratory Orders Error", error },
            { root: true }
        );
    },
    retrieveLaboratoryOrders(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrderResult>> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const laboratoryService: ILaboratoryService =
            container.get<ILaboratoryService>(
                SERVICE_IDENTIFIER.LaboratoryService
            );

        return new Promise((resolve, reject) => {
            const laboratoryOrders: LaboratoryOrder[] =
                context.getters.laboratoryOrders;
            if (context.state.authenticated.status === LoadStatus.LOADED) {
                logger.debug("Laboratory Orders found stored, not querying!");
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        retryin: 0,
                        orders: laboratoryOrders,
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: laboratoryOrders.length,
                });
            } else {
                logger.debug("Retrieving Laboratory Orders");
                context.commit("setLaboratoryOrdersRequested");
                laboratoryService
                    .getLaboratoryOrders(params.hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (result.resultStatus === ResultType.Success) {
                            EventTracker.loadData(
                                EntryType.LaboratoryOrder,
                                result.totalResultCount
                            );
                            context.commit(
                                "setLaboratoryOrders",
                                payload.orders
                            );
                            resolve(result);
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info("Laboratory Orders not loaded");
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
                            context.dispatch(
                                "handleLaboratoryError",
                                result.resultError
                            );
                            reject(result.resultError);
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleLaboratoryError", error);
                        reject(error);
                    });
            }
        });
    },
    handleLaboratoryError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("laboratoryError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Laboratory Orders Error", error },
            { root: true }
        );
    },
    retrievePublicCovidTests(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            collectionDate: StringISODate;
        }
    ): Promise<PublicCovidTestResponseResult> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const laboratoryService: ILaboratoryService =
            container.get<ILaboratoryService>(
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
                        context.dispatch(
                            "handlePublicCovidTestsError",
                            result.resultError
                        );
                        reject(result.resultError);
                    }
                })
                .catch((error) => {
                    context.dispatch("handlePublicCovidTestsError", error);
                    reject(error);
                });
        });
    },
    handlePublicCovidTestsError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        const bannerError: BannerError = {
            title: "Our Apologies",
            description:
                "We've found an issue and the Health Gateway team is working hard to fix it.",
            detail: "",
            errorCode: "",
        };

        if (error.actionCode === ActionType.DataMismatch) {
            bannerError.title = "Data Mismatch";
            bannerError.description =
                "The information you entered does not match our records. Please try again.";
            bannerError.detail =
                "Please note that it can take up to 48 hours from the time of test before a result is available. If it has been at least 48 hours since you tested, please contact the COVID-19 Results Line (1‐833‐707‐2792) to investigate the issue.";
        }

        context.commit("setPublicCovidTestResponseResultError", bannerError);
    },
    resetPublicCovidTestResponseResult(context) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.debug(
            `Resetting Laboratory store module for Public COVID-19 Test response result.`
        );
        context.commit("resetPublicCovidTestResponseResult");
    },
};
