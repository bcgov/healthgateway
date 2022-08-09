import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";

import { VaccinationStatusActions } from "./types";

const setTooManyRequestsWarning = "errorBanner/setTooManyRequestsWarning";

export const actions: VaccinationStatusActions = {
    retrievePublicVaccineStatus(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        return new Promise((resolve, reject) => {
            logger.debug(`Retrieving Vaccination Status`);
            context.commit("setPublicRequested");
            vaccinationStatusService
                .getPublicVaccineStatus(
                    params.phn,
                    params.dateOfBirth,
                    params.dateOfVaccine
                )
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        context.commit("setPublicVaccinationStatus", payload);
                        resolve();
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("VaccinationStatus not loaded");
                        context.commit(
                            "setPublicStatusMessage",
                            "We're busy but will continue to try to fetch your record...."
                        );
                        setTimeout(() => {
                            logger.info("Re-querying for vaccination status");
                            context.dispatch("retrievePublicVaccineStatus", {
                                phn: params.phn,
                                dateOfBirth: params.dateOfBirth,
                                dateOfVaccine: params.dateOfVaccine,
                            });
                        }, payload.retryin);
                        resolve();
                    } else {
                        context.dispatch("handlePublicError", {
                            error: result.resultError,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(result.resultError);
                    }
                })
                .catch((error: ResultError) => {
                    context.dispatch("handlePublicError", {
                        error,
                        errorType: ErrorType.Retrieve,
                    });
                    reject(error);
                });
        });
    },
    handlePublicError(
        context,
        params: { error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);

        if (params.error.statusCode === 429) {
            context.dispatch(
                setTooManyRequestsWarning,
                { key: "publicVaccineCard" },
                { root: true }
            );

            context.commit("publicVaccinationStatusError", undefined);
        } else {
            const customBannerError: CustomBannerError = {
                title: "Our Apologies",
                description:
                    "We've found an issue and the Health Gateway team is working hard to fix it.",
            };

            if (params.error.actionCode === ActionType.DataMismatch) {
                customBannerError.title = "Data Mismatch";
                customBannerError.description = params.error.resultMessage;
            }

            context.commit("publicVaccinationStatusError", customBannerError);
        }
    },
    retrievePublicVaccineRecord(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<CovidVaccineRecord> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        logger.debug(`Retrieving Vaccination Record`);
        context.commit("setPublicVaccineRecordRequested");
        return new Promise((resolve, reject) =>
            vaccinationStatusService
                .getPublicVaccineStatusPdf(
                    params.phn,
                    params.dateOfBirth,
                    params.dateOfVaccine
                )
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        context.commit("setPublicVaccineRecord", payload);
                        resolve(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Public Vaccination Proof not loaded");
                        context.commit(
                            "setPublicVaccineRecordStatusMessage",
                            "We're busy but will continue to try to fetch your proof of vaccination...."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for public proof of vaccination"
                            );
                            context.dispatch("retrievePublicVaccineRecord", {
                                phn: params.phn,
                                dateOfBirth: params.dateOfBirth,
                                dateOfVaccine: params.dateOfVaccine,
                            });
                        }, payload.retryin);
                        resolve(payload);
                    } else {
                        context.dispatch("handlePdfError", {
                            error: result.resultError,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(result.resultError);
                    }
                })
                .catch((error: ResultError) => {
                    context.dispatch("handlePdfError", {
                        error,
                        errorType: ErrorType.Retrieve,
                    });
                    reject(error);
                })
        );
    },
    handlePdfError(
        context,
        params: { error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);

        if (params.error.statusCode === 429) {
            context.dispatch(
                setTooManyRequestsWarning,
                { key: "vaccineCardComponent" },
                { root: true }
            );

            context.commit("setPublicVaccineRecordError", undefined);
        } else {
            const customBannerError: CustomBannerError = {
                title: "Our Apologies",
                description:
                    "We've found an issue and the Health Gateway team is working hard to fix it.",
            };

            context.commit("setPublicVaccineRecordError", customBannerError);
        }
    },
    retrieveAuthenticatedVaccineStatus(
        context,
        params: {
            hdid: string;
        }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        return new Promise((resolve, reject) => {
            if (context.state.authenticated.status === LoadStatus.LOADED) {
                logger.debug(`Vaccination status found stored, not querying!`);
                resolve();
            } else {
                logger.debug(`Retrieving Vaccination Status`);
                context.commit("setAuthenticatedRequested");
                vaccinationStatusService
                    .getAuthenticatedVaccineStatus(params.hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (result.resultStatus === ResultType.Success) {
                            context.commit(
                                "setAuthenticatedVaccinationStatus",
                                payload
                            );
                            resolve();
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info("VaccinationStatus not loaded");
                            context.commit(
                                "setAuthenticatedStatusMessage",
                                "We're busy but will continue to try to fetch your record...."
                            );
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for authenticated proof of vaccination"
                                );
                                context.dispatch(
                                    "retrieveAuthenticatedVaccineStatus",
                                    {
                                        hdid: params.hdid,
                                    }
                                );
                            }, payload.retryin);
                            resolve();
                        } else {
                            context.dispatch("handleAuthenticatedError", {
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                            });
                            reject(result.resultError);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleAuthenticatedError", {
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    handleAuthenticatedError(
        context,
        params: { error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("authenticatedVaccinationStatusError", params.error);

        if (params.error.statusCode === 429) {
            context.dispatch(
                setTooManyRequestsWarning,
                { key: "page" },
                { root: true }
            );
        } else {
            context.dispatch(
                "errorBanner/addError",
                {
                    errorType: params.errorType,
                    source: ErrorSourceType.VaccineCard,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
    retrieveAuthenticatedVaccineRecord(
        context,
        params: {
            hdid: string;
        }
    ): Promise<CovidVaccineRecord> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        return new Promise((resolve, reject) => {
            logger.debug(`Retrieving Vaccination Record`);
            context.commit("setAuthenticatedVaccineRecordRequested");
            vaccinationStatusService
                .getAuthenticatedVaccineRecord(params.hdid)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        context.commit(
                            "setAuthenticatedVaccineRecord",
                            payload
                        );
                        resolve(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Vaccination Record not loaded");
                        context.commit(
                            "setAuthenticatedVaccineRecordStatusMessage",
                            "We're busy but will continue to try to download the Vaccine Record...."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for downloading the Vaccine Record"
                            );
                            context.dispatch(
                                "retrieveAuthenticatedVaccineRecord",
                                {
                                    hdid: params.hdid,
                                }
                            );
                        }, payload.retryin);
                        resolve(payload);
                    } else {
                        context.dispatch("handleAuthenticatedPdfError", {
                            error: result.resultError,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(result.resultError);
                    }
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleAuthenticatedPdfError", {
                        error,
                        errorType: ErrorType.Retrieve,
                    });
                    reject(error);
                });
        });
    },
    handleAuthenticatedPdfError(
        context,
        params: { error: ResultError; errorType: ErrorType }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("setAuthenticatedVaccineRecordError", params.error);

        if (params.error.statusCode === 429) {
            context.dispatch(
                setTooManyRequestsWarning,
                { key: "vaccineCardComponent" },
                { root: true }
            );
        } else {
            if (params.error.actionCode === ActionType.Invalid) {
                context.commit(
                    "setAuthenticatedVaccineRecordResultMessage",
                    "No records found"
                );
            } else {
                context.dispatch(
                    "errorBanner/addError",
                    {
                        errorType: params.errorType,
                        source: ErrorSourceType.VaccineRecord,
                        traceId: params.error.traceId,
                    },
                    { root: true }
                );
            }
        }
    },
};
