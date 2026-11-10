import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import UserFeedback from "@/models/userFeedback";
import {
    IHttpDelegate,
    ILogger,
    IUserFeedbackService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestUserFeedbackService implements IUserFeedbackService {
    private readonly USER_FEEDBACK_BASE_URI: string = "UserFeedback";
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

    public submitFeedback(
        hdid: string,
        feedback: UserFeedback
    ): Promise<boolean> {
        return this.http
            .post<void>(
                `${this.baseUri}${this.USER_FEEDBACK_BASE_URI}/${hdid}`,
                feedback
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserFeedbackService.submitFeedback()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then(() => true);
    }
}
