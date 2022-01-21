import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import {
    AuthenticatedRapidTestRequest,
    AuthenticatedRapidTestResponse,
    LaboratoryOrder,
    LaboratoryReport,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    IHttpDelegate,
    ILaboratoryService,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestLaboratoryService implements ILaboratoryService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly LABORATORY_BASE_URI: string = "v1/api/Laboratory";
    private readonly PUBLIC_LABORATORY_BASE_URI: string =
        "v1/api/PublicLaboratory";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Laboratory"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Laboratory"];
    }

    getCovidTests(
        phn: string,
        dateOfBirth: string,
        collectionDate: string
    ): Promise<RequestResult<PublicCovidTestResponseResult>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }
            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["collectionDate"] = collectionDate;
            this.http
                .getWithCors<RequestResult<PublicCovidTestResponseResult>>(
                    `${this.baseUri}${this.PUBLIC_LABORATORY_BASE_URI}/CovidTests`,
                    headers
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`getCovidTests Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Laboratory
                        )
                    );
                });
        });
    }

    public getOrders(hdid: string): Promise<RequestResult<LaboratoryOrder[]>> {
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
                .getWithCors<RequestResult<LaboratoryOrder[]>>(
                    `${this.baseUri}${this.LABORATORY_BASE_URI}/Covid19Orders?hdid=${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`getOrders Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Laboratory
                        )
                    );
                });
        });
    }

    public getReportDocument(
        reportId: string,
        hdid: string
    ): Promise<RequestResult<LaboratoryReport>> {
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
                .getWithCors<RequestResult<LaboratoryReport>>(
                    `${this.baseUri}${this.LABORATORY_BASE_URI}/${reportId}/Report?hdid=${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`getReportDocument Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Laboratory
                        )
                    );
                });
        });
    }

    public postAuthenticatedRapidTest(
        hdid: string,
        request: AuthenticatedRapidTestRequest
    ): Promise<RequestResult<AuthenticatedRapidTestResponse>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            this.http
                .post<RequestResult<AuthenticatedRapidTestResponse>>(
                    `${this.baseUri}${this.LABORATORY_BASE_URI}/${hdid}/rapidTest`,
                    request
                )
                .then((requestResult) => {
                    resolve(requestResult);
                    this.logger.debug(
                        `CreateRapidTest ${requestResult.resultStatus}`
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `Post Autheticate Rapid Test Error: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
