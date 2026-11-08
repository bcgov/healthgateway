import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type {
    Covid19LaboratoryOrderResult,
    LaboratoryOrderResult,
    LaboratoryReport,
} from "@/models/laboratory";
import type { RequestResult } from "@/models/requestResult";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface ILaboratoryService {
    getCovid19LaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>>;
    getLaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>>;
    getReportDocument(
        reportId: string,
        hdid: string,
        isCovid19: boolean
    ): Promise<RequestResult<LaboratoryReport>>;
}

const empty = <T>(resourcePayload: T): RequestResult<T> => ({
    pageIndex: 0,
    pageSize: 0,
    resourcePayload,
    resultStatus: ResultType.Success,
    totalResultCount: 0,
});

/** Port of services/restLaboratoryService.ts (lab results and COVID-19 tests). */
export class LaboratoryService implements ILaboratoryService {
    private readonly LABORATORY_BASE_URI = "Laboratory";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["Laboratory"];
    }

    async getCovid19LaboratoryOrders(hdid: string) {
        if (!ConfigUtil.isDatasetEnabled(EntryType.Covid19TestResult))
            return empty<Covid19LaboratoryOrderResult>({
                loaded: true,
                retryin: 0,
                orders: [],
            });
        try {
            return await this.http.get<
                RequestResult<Covid19LaboratoryOrderResult>
            >(
                `${this.baseUri}${this.LABORATORY_BASE_URI}/Covid19Orders?hdid=${hdid}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestLaboratoryService.getCovid19LaboratoryOrders()"
            );
            throw toResultError(error, ServiceCode.Laboratory);
        }
    }

    async getLaboratoryOrders(hdid: string) {
        if (!ConfigUtil.isDatasetEnabled(EntryType.LabResult))
            return empty<LaboratoryOrderResult>({
                loaded: true,
                queued: false,
                retryin: 0,
                orders: [],
            });
        try {
            return await this.http.get<RequestResult<LaboratoryOrderResult>>(
                `${this.baseUri}${this.LABORATORY_BASE_URI}/LaboratoryOrders?hdid=${hdid}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestLaboratoryService.getLaboratoryOrders()"
            );
            throw toResultError(error, ServiceCode.Laboratory);
        }
    }

    async getReportDocument(
        reportId: string,
        hdid: string,
        isCovid19: boolean
    ) {
        const labEnabled = ConfigUtil.isDatasetEnabled(EntryType.LabResult);
        const covidEnabled = ConfigUtil.isDatasetEnabled(
            EntryType.Covid19TestResult
        );
        if ((!labEnabled && !isCovid19) || (!covidEnabled && isCovid19))
            return empty<LaboratoryReport>({
                data: "",
                encoding: "",
                mediaType: "",
            });
        try {
            return await this.http.get<RequestResult<LaboratoryReport>>(
                `${this.baseUri}${this.LABORATORY_BASE_URI}/${reportId}/Report?hdid=${hdid}&isCovid19=${isCovid19}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestLaboratoryService.getReportDocument()");
            throw toResultError(error, ServiceCode.Laboratory);
        }
    }
}
