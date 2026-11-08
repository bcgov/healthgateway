import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { Report, ReportRequest, RequestResult } from "@/models/report";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IReportService {
    generateReport(
        reportRequest: ReportRequest
    ): Promise<RequestResult<Report>>;
}

/** Port of services/restReportService.ts */
export class ReportService implements IReportService {
    private readonly REPORT_BASE_URI = "Report";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    async generateReport(
        reportRequest: ReportRequest
    ): Promise<RequestResult<Report>> {
        try {
            return await this.http.post<RequestResult<Report>>(
                `${this.baseUri}${this.REPORT_BASE_URI}`,
                reportRequest
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestReportService.generateReport()");
            throw toResultError(error, ServiceCode.Report);
        }
    }
}
