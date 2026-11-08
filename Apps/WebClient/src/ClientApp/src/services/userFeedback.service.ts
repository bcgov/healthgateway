import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { UserFeedback, UserRating } from "@/models/userFeedback";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IUserFeedbackService {
    submitFeedback(hdid: string, feedback: UserFeedback): Promise<void>;
    submitRating(rating: UserRating): Promise<void>;
}

export class UserFeedbackService implements IUserFeedbackService {
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = `${config.serviceEndpoints["GatewayApi"]}UserFeedback`;
    }

    async submitFeedback(hdid: string, feedback: UserFeedback) {
        try {
            await this.http.post<void>(`${this.baseUri}/${hdid}`, feedback);
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in UserFeedbackService.submitFeedback()",
                error
            );
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }

    async submitRating(rating: UserRating) {
        try {
            await this.http.post<void>(`${this.baseUri}/Rating`, rating);
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in UserFeedbackService.submitRating()", error);
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }
}
