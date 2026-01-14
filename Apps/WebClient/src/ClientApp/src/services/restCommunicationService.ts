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

export class RestCommunicationService implements ICommunicationService {
    private readonly BASE_URI: string = "Communication";
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

    public getActive(
        type: CommunicationType
    ): Promise<RequestResult<Communication | null>> {
        return this.http
            .getWithCors<
                RequestResult<Communication | null>
            >(`${this.baseUri}${this.BASE_URI}/${type}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestCommunicationService.getActive()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }
}
