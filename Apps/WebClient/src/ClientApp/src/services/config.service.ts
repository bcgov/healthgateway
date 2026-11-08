import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { describeError, logger } from "./logger";

export interface IConfigService {
    getConfiguration(url: string): Promise<ExternalConfiguration>;
}

export class ConfigService implements IConfigService {
    constructor(private readonly http: IHttpClient) {}

    async getConfiguration(url: string): Promise<ExternalConfiguration> {
        try {
            return await this.http.getPublic<ExternalConfiguration>(url);
        } catch (error) {
            // the caller decides: the start-up error page, a saved copy, or trying again later
            logger.info(
                `ConfigService.getConfiguration() failed: ${describeError(error)}`
            );
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }
}
