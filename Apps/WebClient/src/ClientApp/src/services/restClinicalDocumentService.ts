import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import ClinicalDocument from "@/models/clinicalDocument";
import { ExternalConfiguration } from "@/models/configData";
import EncodedMedia from "@/models/encodedMedia";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IClinicalDocumentService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestClinicalDocumentService implements IClinicalDocumentService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly BASE_URI = "ClinicalDocument";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["ClinicalDocument"];
        this.http = http;
        this.isEnabled = config.webClient.modules["ClinicalDocument"];
    }

    public getRecords(
        hdid: string
    ): Promise<RequestResult<ClinicalDocument[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: [],
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<ClinicalDocument[]>>(
                    `${this.baseUri}${this.BASE_URI}/${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestClinicalDocumentService.getRecords()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.ClinicalDocument
                        )
                    );
                });
        });
    }

    public getFile(
        fileId: string,
        hdid: string
    ): Promise<RequestResult<EncodedMedia>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: { data: "", encoding: "", mediaType: "" },
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<EncodedMedia>>(
                    `${this.baseUri}${this.BASE_URI}/${hdid}/file/${fileId}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestClinicalDocumentService.getFile()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.ClinicalDocument
                        )
                    );
                });
        });
    }
}
