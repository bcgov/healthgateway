import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ClinicalDocument } from "@/models/clinicalDocument";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { EncodedMedia } from "@/models/laboratory";
import type { RequestResult } from "@/models/requestResult";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IClinicalDocumentService {
    getRecords(hdid: string): Promise<RequestResult<ClinicalDocument[]>>;
    getFile(fileId: string, hdid: string): Promise<RequestResult<EncodedMedia>>;
}

/** Port of services/restClinicalDocumentService.ts */
export class ClinicalDocumentService implements IClinicalDocumentService {
    private readonly BASE_URI = "ClinicalDocument";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["ClinicalDocument"];
    }

    async getRecords(hdid: string): Promise<RequestResult<ClinicalDocument[]>> {
        if (!ConfigUtil.isDatasetEnabled(EntryType.ClinicalDocument))
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        try {
            return await this.http.get<RequestResult<ClinicalDocument[]>>(
                `${this.baseUri}${this.BASE_URI}/${hdid}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestClinicalDocumentService.getRecords()");
            throw toResultError(error, ServiceCode.ClinicalDocument);
        }
    }

    async getFile(
        fileId: string,
        hdid: string
    ): Promise<RequestResult<EncodedMedia>> {
        if (!ConfigUtil.isDatasetEnabled(EntryType.ClinicalDocument))
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: { data: "", encoding: "", mediaType: "" },
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        try {
            return await this.http.get<RequestResult<EncodedMedia>>(
                `${this.baseUri}${this.BASE_URI}/${hdid}/file/${fileId}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestClinicalDocumentService.getFile()");
            throw toResultError(error, ServiceCode.ClinicalDocument);
        }
    }
}
