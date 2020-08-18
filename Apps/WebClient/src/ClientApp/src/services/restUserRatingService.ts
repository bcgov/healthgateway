import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IUserRatingService,
} from "@/services/interfaces";
import UserRating from "@/models/userRating";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";

@injectable()
export class RestUserRatingService implements IUserRatingService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_RATING_BASE_URI: string = "/v1/api/Rating";
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
