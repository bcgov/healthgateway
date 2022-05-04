import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import Report from "@/models/report";
import ReportRequest from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger, IReportService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestReportService implements IReportService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly REPORT_BASE_URI: string = "/v1/api/Report";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public generateReport(
        reportRequest: ReportRequest
    ): Promise<RequestResult<Report>> {
        return new Promise((resolve, reject) => {
            this.http
                .post<RequestResult<Report>>(
                    `${this.baseUri}${this.REPORT_BASE_URI}`,
                    reportRequest
                )
                .then((result) => {
                    return resolve(result);
                })
                .catch((err) => {
                    this.logger.error(`generateReport Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Report
                        )
                    );
                });
        });
    }
}
