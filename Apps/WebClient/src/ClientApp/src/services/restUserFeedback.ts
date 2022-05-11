import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
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
    private readonly USER_FEEDBACK_BASE_URI: string = "v1/api/UserFeedback";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public submitFeedback(
        hdid: string,
        feedback: UserFeedback
    ): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .post<void>(
                    `${this.baseUri}${this.USER_FEEDBACK_BASE_URI}/${hdid}`,
                    feedback
                )
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
