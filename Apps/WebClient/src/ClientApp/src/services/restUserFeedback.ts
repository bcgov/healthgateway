import { injectable } from "inversify";

import { ServiceName } from "@/models/errorInterfaces";
import UserFeedback from "@/models/userFeedback";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserFeedbackService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestUserFeedbackService implements IUserFeedbackService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_FEEDBACK_BASE_URI: string = "/v1/api/UserFeedback";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public submitFeedback(
        hdid: string,
        feedback: UserFeedback
    ): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .post<void>(`${this.USER_FEEDBACK_BASE_URI}/${hdid}`, feedback)
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
