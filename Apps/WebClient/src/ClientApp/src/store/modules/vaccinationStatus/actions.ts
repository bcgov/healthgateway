import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

import { VaccinationStatusActions } from "./types";

export const actions: VaccinationStatusActions = {
    retrieveVaccineStatus(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService: IVaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        return new Promise((resolve, reject) => {
            logger.debug(`Retrieving Vaccination Status`);
            context.commit("setRequested");
            vaccinationStatusService
                .getPublicVaccineStatus(
                    params.phn,
                    params.dateOfBirth,
                    params.dateOfVaccine
                )
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        context.commit("setVaccinationStatus", payload);
                        resolve();
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("VaccinationStatus not loaded");
                        context.commit(
                            "setStatusMessage",
                            "We're busy but will continue to try to fetch your record...."
                        );
                        setTimeout(() => {
                            logger.info("Re-querying for vaccination status");
                            context.dispatch("retrieveVaccineStatus", {
                                phn: params.phn,
                                dateOfBirth: params.dateOfBirth,
                                dateOfVaccine: params.dateOfVaccine,
                            });
                        }, payload.retryin);
                        resolve();
                    } else {
                        context.dispatch("handleError", result.resultError);
                        reject(result.resultError);
                    }
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    handleError(context, error: ResultError) {
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
            bannerError.description = error.resultMessage;
        }

        context.commit("vaccinationStatusError", bannerError);
    },
    retrievePublicVaccineRecord(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<CovidVaccineRecord> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.debug(`Retrieving Vaccination Record`);
        context.commit("setPublicVaccineRecordRequested");
        const vaccinationStatusService: IVaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        return new Promise((resolve, reject) => {
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
                        context.dispatch("handlePdfError", result.resultError);
                        reject(result.resultError);
                    }
                })
                .catch((error) => {
                    context.dispatch("handlePdfError", error);
                    reject(error);
                });
        });
    },
    handlePdfError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        const bannerError: BannerError = {
            title: "Our Apologies",
            description:
                "We've found an issue and the Health Gateway team is working hard to fix it.",
            detail: "",
            errorCode: "",
        };

        context.commit("setPublicVaccineRecordError", bannerError);
    },
    retrieveAuthenticatedVaccineStatus(
        context,
        params: {
            hdid: string;
        }
    ): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService: IVaccinationStatusService =
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
                            context.dispatch(
                                "handleAuthenticatedError",
                                result.resultError
                            );
                            reject(result.resultError);
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleAuthenticatedError", error);
                        reject(error);
                    });
            }
        });
    },
    handleAuthenticatedError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(error)}`);

        const title = "Error Retrieving Vaccine Card";

        context.commit(
            "authenticatedVaccinationStatusError",
            ErrorTranslator.toBannerError(title, error)
        );
        context.dispatch(
            "errorBanner/addResultError",
            { message: title, error },
            { root: true }
        );
    },
    retrieveAuthenticatedVaccineRecord(
        context,
        params: {
            hdid: string;
        }
    ): Promise<CovidVaccineRecord> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService: IVaccinationStatusService =
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
                        context.dispatch(
                            "handleAuthenticatedPdfError",
                            result.resultError
                        );
                        reject(result.resultError);
                    }
                })
                .catch((error) => {
                    context.dispatch("handleAuthenticatedPdfError", error);
                    reject(error);
                });
        });
    },
    handleAuthenticatedPdfError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        const title = "Error Retrieving Vaccine Record";

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit(
            "setAuthenticatedVaccineRecordError",
            ErrorTranslator.toBannerError(title, error)
        );

        if (error.actionCode === ActionType.Invalid) {
            context.commit(
                "setAuthenticatedVaccineRecordResultMessage",
                "No records found"
            );
        } else {
            context.dispatch(
                "errorBanner/addResultError",
                { message: title, error },
                { root: true }
            );
        }
    },
};
