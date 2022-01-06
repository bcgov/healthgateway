import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import { StringISODate } from "@/models/dateWrapper";
import {
    LaboratoryOrder,
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
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrder[]>> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const laboratoryService: ILaboratoryService =
            container.get<ILaboratoryService>(
                SERVICE_IDENTIFIER.LaboratoryService
            );

        return new Promise((resolve, reject) => {
            const laboratoryOrders: LaboratoryOrder[] =
                context.getters.laboratoryOrders;
            if (context.state.authenticated.status === LoadStatus.LOADED) {
                logger.debug(`Laboratory found stored, not querying!`);
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
                            EventTracker.loadData(
                                EntryType.Laboratory,
                                result.resourcePayload.length
                            );
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
                .getCovidTests(
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
                "Please note that it can take up to 48 hours from the time of test before a result is available.";
        }

        context.commit("setPublicCovidTestResponseResultError", bannerError);
    },
    resetPublicCovidTestResponseResult(context) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.debug(
            `Re-setting Laboratory store module for Public Covid Test response result.`
        );
        context.commit("resetPublicCovidTestResponseResult");
    },
};
