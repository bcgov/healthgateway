import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";
import {
    IHttpDelegate,
    ILogger,
    IUserProfileService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

export class RestUserProfileService implements IUserProfileService {
    private readonly USER_PROFILE_BASE_URI: string = "UserProfile";
    private logger;
    private http;
    private baseUri;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public getTermsOfService(): Promise<TermsOfService> {
        return this.http
            .get<RequestResult<TermsOfService>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/termsofservice`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.getTermsOfService()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `getTermsOfService ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }
}
