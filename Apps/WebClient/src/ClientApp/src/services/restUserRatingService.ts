import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import UserRating from "@/models/userRating";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserRatingService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestUserRatingService implements IUserRatingService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_RATING_BASE_URI: string = "UserFeedback/Rating";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public submitRating(rating: UserRating): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .post<void>(
                    `${this.baseUri}${this.USER_RATING_BASE_URI}`,
                    rating
                )
                .then(() => {
                    return resolve(true);
                })
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
                });
        });
    }
}
