import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IUserFeedbackService,
} from "@/services/interfaces";
import UserFeedback from "@/models/userFeedback";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";

@injectable()
export class RestUserFeedbackService implements IUserFeedbackService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_FEEDBACK_BASE_URI: string = "/v1/api/UserFeedback";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public submitFeedback(feedback: UserFeedback): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .post<void>(this.USER_FEEDBACK_BASE_URI, feedback)
                .then(() => {
                    return resolve(true);
                })
                .catch((err) => {
                    this.logger.error(`submitFeedback Fetch error: ${err}`);
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
