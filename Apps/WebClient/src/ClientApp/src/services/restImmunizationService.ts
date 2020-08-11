import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IImmunizationService,
} from "@/services/interfaces";
import ImmunizationData from "@/models/immunizationData";
import { ExternalConfiguration } from "@/models/configData";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";

@injectable()
export class RestImmunizationService implements IImmunizationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly IMMS_BASE_URI: string = "v1/api/Immunization";
    private baseUri: string = "";
    private http!: IHttpDelegate;
    private isEnabled: boolean = false;

    constructor() {}

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Immunization"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Immunization"];
    }

    public getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationData[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: [],
                    resultMessage: "",
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                    errorCode: "",
                });
                return;
            }

            this.http
                .getWithCors<RequestResult<ImmunizationData[]>>(
                    `${this.baseUri}${this.IMMS_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch error: ${err}`);
                    reject(err);
                });
        });
    }
}
