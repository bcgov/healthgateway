import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import { VaccineProofTemplate } from "@/constants/vaccineProofTemplate";
import BannerError from "@/models/bannerError";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import Report from "@/models/report";
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
                    if (result.resultStatus === ResultType.Success) {
                        const payload = result.resourcePayload;
                        if (!payload.loaded && payload.retryin > 0) {
                            logger.info("VaccinationStatus not loaded");
                            context.commit(
                                "setStatusMessage",
                                "We're busy but will continue to try to fetch your record...."
                            );
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for vaccination status"
                                );
                                context.dispatch("retrieveVaccineStatus", {
                                    phn: params.phn,
                                    dateOfBirth: params.dateOfBirth,
                                    dateOfVaccine: params.dateOfVaccine,
                                });
                            }, payload.retryin);
                            resolve();
                        } else {
                            context.commit("setVaccinationStatus", payload);
                            resolve();
                        }
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
    retrieveVaccineStatusPdf(
        context,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<Report> {
        context.commit("setPdfRequested");

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
                    if (result.resultStatus === ResultType.Success) {
                        const payload = result.resourcePayload;
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

        context.commit("pdfError", bannerError);
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
                        if (result.resultStatus === ResultType.Success) {
                            const payload = result.resourcePayload;
                            if (!payload.loaded && payload.retryin > 0) {
                                logger.info("VaccinationStatus not loaded");
                                context.commit(
                                    "setAuthenticatedStatusMessage",
                                    "We're busy but will continue to try to fetch your record...."
                                );
                                setTimeout(() => {
                                    logger.info(
                                        "Re-querying for vaccination status"
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
                                context.commit(
                                    "setAuthenticatedVaccinationStatus",
                                    payload
                                );
                                resolve();
                            }
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
    retrieveAuthenticatedVaccineRecord(
        context,
        params: {
            hdid: string;
            proofTemplate: VaccineProofTemplate;
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
                .getAuthenticatedVaccineRecord(
                    params.hdid,
                    params.proofTemplate
                )
                .then((result) => {
                    if (result.resultStatus === ResultType.Success) {
                        const payload = result.resourcePayload;
                        if (!payload.loaded && payload.retryin > 0) {
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
                            context.commit(
                                "setAuthenticatedVaccineRecord",
                                payload
                            );
                            resolve(payload);
                        }
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
    handleAuthenticatedPdfError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        const title = "Error Retrieving Vaccine Record";

        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit(
            "setAuthenticatedVaccineRecordError",
            ErrorTranslator.toBannerError(title, error)
        );
        context.dispatch(
            "errorBanner/addResultError",
            { message: title, error },
            { root: true }
        );
    },
};
