import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IConfigService, IHttpDelegate, ILogger } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestConfigService implements IConfigService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly CONFIG_BASE_URI: string = "/configuration";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getConfiguration(): Promise<ExternalConfiguration> {
        return new Promise((resolve, reject) =>
            this.http
                .getWithCors<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
                .then((result) => resolve(result))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestConfigService.getConfiguration()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
}
