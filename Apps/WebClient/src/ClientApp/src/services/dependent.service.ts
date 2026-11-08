import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import type { AddDependentRequest, Dependent } from "@/models/dependent";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { RequestResult } from "@/models/requestResult";
import { handleResult } from "@/utils/requestResult";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IDependentService {
    addDependent(
        hdid: string,
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest | undefined>;
    getAll(hdid: string): Promise<Dependent[]>;
    removeDependent(hdid: string, dependent: Dependent): Promise<void>;
}

/** Port of services/restDependentService.ts */
export class DependentService implements IDependentService {
    private readonly DEPENDENT_BASE_URI = "UserProfile";
    private readonly baseUri: string;
    private readonly isEnabled: boolean;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["GatewayApi"];
        this.isEnabled =
            config.webClient.featureToggleConfiguration.dependents.enabled;
    }

    private url(hdid: string) {
        return `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent`;
    }

    async addDependent(
        hdid: string,
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest | undefined> {
        if (!this.isEnabled) return undefined;
        let result: RequestResult<AddDependentRequest>;
        try {
            result = await this.http.post<RequestResult<AddDependentRequest>>(
                this.url(hdid),
                dependent
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestDependentService.addDependent()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
        return handleResult(result);
    }

    async getAll(hdid: string): Promise<Dependent[]> {
        let result: RequestResult<Dependent[]>;
        try {
            result = await this.http.get<RequestResult<Dependent[]>>(
                this.url(hdid)
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestDependentService.getAll()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
        return handleResult(result);
    }

    async removeDependent(hdid: string, dependent: Dependent): Promise<void> {
        let result: RequestResult<void>;
        try {
            result = await this.http.delete<RequestResult<void>>(
                `${this.url(hdid)}/${dependent.ownerId}`,
                dependent
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestDependentService.removeDependent()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
        handleResult(result);
    }
}
