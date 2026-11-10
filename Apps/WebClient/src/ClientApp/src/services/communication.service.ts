import { ServiceCode } from "@/constants/errorType";
import type {
    Communication,
    CommunicationType,
    RequestResult,
} from "@/models/communication";
import type { ExternalConfiguration } from "@/models/configData";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { describeError, logger } from "./logger";

export interface ICommunicationService {
    getActive(
        type: CommunicationType
    ): Promise<RequestResult<Communication | null>>;
}

export class CommunicationService implements ICommunicationService {
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = `${config.serviceEndpoints["GatewayApi"]}Communication`;
    }

    async getActive(type: CommunicationType) {
        try {
            return await this.http.getPublic<
                RequestResult<Communication | null>
            >(`${this.baseUri}/${type}`);
        } catch (error) {
            // the banner is optional and asks again on the next page: not worth a console error
            logger.info(
                `CommunicationService.getActive() failed: ${describeError(error)}`
            );
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }
}
