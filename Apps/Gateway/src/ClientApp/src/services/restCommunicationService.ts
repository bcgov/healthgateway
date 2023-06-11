import { ServiceCode } from "@/constants/serviceCodes";
import Communication, { CommunicationType } from "@/models/communication";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import {
    ICommunicationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import { WinstonLogger } from "@/services/winstonLogger";

export class RestCommunicationService implements ICommunicationService {
    private logger: ILogger = new WinstonLogger(true); // TODO: inject logger
    private readonly BASE_URI: string = "Communication";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public getActive(
        type: CommunicationType
    ): Promise<RequestResult<Communication>> {
        return new Promise((resolve, reject) =>
            this.http
                .getWithCors<RequestResult<Communication>>(
                    `${this.baseUri}${this.BASE_URI}/${type}`
                )
                .then((communication) => resolve(communication))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestCommunicationService.getActive()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
}
