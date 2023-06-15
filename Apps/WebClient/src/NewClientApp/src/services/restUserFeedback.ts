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

    public submitFeedback(
        hdid: string,
        feedback: UserFeedback
    ): Promise<boolean> {
        return new Promise((resolve, reject) =>
            this.http
                .post<void>(
                    `${this.baseUri}${this.USER_FEEDBACK_BASE_URI}/${hdid}`,
                    feedback
                )
                .then(() => resolve(true))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserFeedbackService.submitFeedback()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
}
