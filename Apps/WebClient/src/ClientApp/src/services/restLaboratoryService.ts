import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    ILaboratoryService,
} from "@/services/interfaces";
import { ExternalConfiguration } from "@/models/configData";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";

@injectable()
export class RestLaboratoryService implements ILaboratoryService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly LABORATORY_BASE_URI: string = "v1/api/Laboratory";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    constructor() {}

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Laboratory"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Laboratory"];
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
                    `${this.baseUri}${this.LABORATORY_BASE_URI}?hdid=${hdid}`
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
}
