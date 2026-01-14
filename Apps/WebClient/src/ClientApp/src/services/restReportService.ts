import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import Report from "@/models/report";
import ReportRequest from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { IHttpDelegate, ILogger, IReportService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestReportService implements IReportService {
    private readonly REPORT_BASE_URI: string = "Report";
    private readonly logger;
    private readonly http;
    private readonly baseUri;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public generateReport(
        reportRequest: ReportRequest
    ): Promise<RequestResult<Report>> {
        return this.http
            .post<
                RequestResult<Report>
            >(`${this.baseUri}${this.REPORT_BASE_URI}`, reportRequest)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestReportService.generateReport()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Report
                );
            });
    }
}
