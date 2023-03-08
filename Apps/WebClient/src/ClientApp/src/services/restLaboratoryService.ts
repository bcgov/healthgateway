import { injectable } from "inversify";

import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import {
    Covid19LaboratoryOrderResult,
    LaboratoryOrderResult,
    LaboratoryReport,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILaboratoryService,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestLaboratoryService implements ILaboratoryService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly LABORATORY_BASE_URI: string = "Laboratory";
    private readonly PUBLIC_LABORATORY_BASE_URI: string = "PublicLaboratory";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private isCovid19Enabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Laboratory"];
        this.http = http;
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.LabResult);
        this.isCovid19Enabled = ConfigUtil.isDatasetEnabled(
            EntryType.Covid19TestResult
        );
    }

    getPublicCovid19Tests(
        phn: string,
        dateOfBirth: string,
        collectionDate: string
    ): Promise<RequestResult<PublicCovidTestResponseResult>> {
        return new Promise((resolve, reject) => {
            if (!this.isCovid19Enabled) {
                reject(
                    ErrorTranslator.moduleDisabledError(ServiceCode.Laboratory)
                );
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
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestLaboratoryService.getPublicCovid19Tests()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Laboratory
                        )
                    );
                });
        });
    }

    public getCovid19LaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>> {
        return new Promise((resolve, reject) => {
            if (!this.isCovid19Enabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: { loaded: true, retryin: 0, orders: [] },
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<Covid19LaboratoryOrderResult>>(
                    `${this.baseUri}${this.LABORATORY_BASE_URI}/Covid19Orders?hdid=${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestLaboratoryService.getCovid19LaboratoryOrders()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Laboratory
                        )
                    );
                });
        });
    }

    public getLaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        queued: false,
                        retryin: 0,
                        orders: [],
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<LaboratoryOrderResult>>(
                    `${this.baseUri}${this.LABORATORY_BASE_URI}/LaboratoryOrders?hdid=${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestLaboratoryService.getLaboratoryOrders()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Laboratory
                        )
                    );
                });
        });
    }

    public getReportDocument(
        reportId: string,
        hdid: string,
        isCovid19: boolean
    ): Promise<RequestResult<LaboratoryReport>> {
        return new Promise((resolve, reject) => {
            if (
                (!this.isEnabled && !isCovid19) ||
                (!this.isCovid19Enabled && isCovid19)
            ) {
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
                    `${this.baseUri}${this.LABORATORY_BASE_URI}/${reportId}/Report?hdid=${hdid}&isCovid19=${isCovid19}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestLaboratoryService.getReportDocument()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Laboratory
                        )
                    );
                });
        });
    }
}
