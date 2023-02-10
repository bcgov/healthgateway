import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ClinicalDocument } from "@/models/clinicalDocument";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IClinicalDocumentService, ILogger } from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import { ClinicalDocumentActions } from "./types";
import { getClinicalDocumentDatasetState } from "./util";

export const actions: ClinicalDocumentActions = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<RequestResult<ClinicalDocument[]>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const clinicalDocumentService = container.get<IClinicalDocumentService>(
            SERVICE_IDENTIFIER.ClinicalDocumentService
        );

        return new Promise((resolve, reject) => {
            if (
                getClinicalDocumentDatasetState(context.state, params.hdid)
                    .status === LoadStatus.LOADED
            ) {
                logger.debug("Clinical documents found stored, not querying!");
                const records: ClinicalDocument[] = context.getters.records(
                    params.hdid
                );
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: records,
                    resultStatus: ResultType.Success,
                    totalResultCount: records.length,
                });
            } else {
                logger.debug("Retrieving clinical documents");
                context.commit("setRequested", params.hdid);
                clinicalDocumentService
                    .getRecords(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", {
                                error: result.resultError,
                                errorType: ErrorType.Retrieve,
                                hdid: params.hdid,
                            });
                            reject(result.resultError);
                        } else {
                            EventTracker.loadData(
                                EntryType.ClinicalDocument,
                                result.resourcePayload.length
                            );
                            context.commit("setRecords", {
                                hdid: params.hdid,
                                records: result.resourcePayload,
                            });
                            resolve(result);
                        }
                    })
                    .catch((error: ResultError) => {
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
    getFile(
        context,
        params: {
            fileId: string;
            hdid: string;
        }
    ): Promise<EncodedMedia> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const clinicalDocumentService = container.get<IClinicalDocumentService>(
            SERVICE_IDENTIFIER.ClinicalDocumentService
        );

        return new Promise((resolve, reject) => {
            logger.debug(`Retrieving clinical document file`);
            context.commit("setFileRequested", params.fileId);
            clinicalDocumentService
                .getFile(params.fileId, params.hdid)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        context.commit("setFile", {
                            fileId: params.fileId,
                            file: payload,
                        });
                        resolve(payload);
                    } else {
                        context.dispatch("handleError", {
                            error: result.resultError,
                            errorType: ErrorType.Download,
                            fileId: params.fileId,
                        });
                        reject(result.resultError);
                    }
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Download,
                        fileId: params.fileId,
                    });
                    reject(error);
                });
        });
    },
    handleError(
        context,
        params: {
            error: ResultError;
            errorType: ErrorType;
            hdid?: string;
            fileId?: string;
        }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        if (params.fileId) {
            context.commit("setFileError", {
                fileId: params.fileId,
                error: params.error,
            });
        } else {
            context.commit("setError", {
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
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
