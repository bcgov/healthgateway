import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ClinicalDocument } from "@/models/clinicalDocument";
import { ExternalConfiguration } from "@/models/configData";
import EncodedMedia from "@/models/encodedMedia";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import {
    IClinicalDocumentService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestClinicalDocumentService implements IClinicalDocumentService {
    private readonly BASE_URI = "ClinicalDocument";
    private readonly logger;
    private readonly http;
    private readonly baseUri;
    private readonly isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["ClinicalDocument"];
        this.isEnabled = ConfigUtil.isDatasetEnabled(
            EntryType.ClinicalDocument
        );
    }

    public getRecords(
        hdid: string
    ): Promise<RequestResult<ClinicalDocument[]>> {
        if (!this.isEnabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .getWithCors<
                RequestResult<ClinicalDocument[]>
            >(`${this.baseUri}${this.BASE_URI}/${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestClinicalDocumentService.getRecords()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.ClinicalDocument
                );
            });
    }

    public getFile(
        fileId: string,
        hdid: string
    ): Promise<RequestResult<EncodedMedia>> {
        if (!this.isEnabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: { data: "", encoding: "", mediaType: "" },
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .getWithCors<
                RequestResult<EncodedMedia>
            >(`${this.baseUri}${this.BASE_URI}/${hdid}/file/${fileId}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestClinicalDocumentService.getFile()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.ClinicalDocument
                );
            });
    }
}
