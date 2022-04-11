import { injectable } from "inversify";

import { ServiceName } from "@/models/errorInterfaces";
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
    private readonly USER_RATING_BASE_URI: string =
        "/v1/api/UserFeedback/Rating";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public submitRating(rating: UserRating): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .post<void>(this.USER_RATING_BASE_URI, rating)
                .then(() => {
                    return resolve(true);
                })
                .catch((err) => {
                    this.logger.error(`submitRating Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
