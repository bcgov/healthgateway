import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import UserRating from "@/models/userRating";
import {
    IHttpDelegate,
    ILogger,
    IUserRatingService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestUserRatingService implements IUserRatingService {
    private readonly USER_RATING_BASE_URI: string = "UserFeedback/Rating";
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

    public submitRating(rating: UserRating): Promise<boolean> {
        return new Promise((resolve, reject) =>
            this.http
                .post<void>(
                    `${this.baseUri}${this.USER_RATING_BASE_URI}`,
                    rating
                )
                .then(() => resolve(true))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserRatingService.submitRating()`
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
