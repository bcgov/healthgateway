import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import {
    Covid19LaboratoryOrderResult,
    LaboratoryOrderResult,
    LaboratoryReport,
} from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import {
    IHttpDelegate,
    ILaboratoryService,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestLaboratoryService implements ILaboratoryService {
    private readonly LABORATORY_BASE_URI: string = "Laboratory";
    private readonly logger;
    private readonly http;
    private readonly baseUri;
    private readonly isEnabled;
    private readonly isCovid19Enabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["Laboratory"];
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.LabResult);
        this.isCovid19Enabled = ConfigUtil.isDatasetEnabled(
            EntryType.Covid19TestResult
        );
    }

    public getCovid19LaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>> {
        if (!this.isCovid19Enabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: { loaded: true, retryin: 0, orders: [] },
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .getWithCors<
                RequestResult<Covid19LaboratoryOrderResult>
            >(`${this.baseUri}${this.LABORATORY_BASE_URI}/Covid19Orders?hdid=${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestLaboratoryService.getCovid19LaboratoryOrders()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Laboratory
                );
            });
    }

    public getLaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>> {
        if (!this.isEnabled) {
            return Promise.resolve({
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
        }

        return this.http
            .getWithCors<
                RequestResult<LaboratoryOrderResult>
            >(`${this.baseUri}${this.LABORATORY_BASE_URI}/LaboratoryOrders?hdid=${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestLaboratoryService.getLaboratoryOrders()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Laboratory
                );
            });
    }

    public getReportDocument(
        reportId: string,
        hdid: string,
        isCovid19: boolean
    ): Promise<RequestResult<LaboratoryReport>> {
        if (
            (!this.isEnabled && !isCovid19) ||
            (!this.isCovid19Enabled && isCovid19)
        ) {
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
                RequestResult<LaboratoryReport>
            >(`${this.baseUri}${this.LABORATORY_BASE_URI}/${reportId}/Report?hdid=${hdid}&isCovid19=${isCovid19}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestLaboratoryService.getReportDocument()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Laboratory
                );
            });
    }
}
