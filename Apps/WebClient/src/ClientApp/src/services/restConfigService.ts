import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { IConfigService, IHttpDelegate, ILogger } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestConfigService implements IConfigService {
    private logger;
    private readonly CONFIG_BASE_URI: string = import.meta.env.VITE_CONFIG_BASE_URI || "/configuration";
    private http;

    constructor(logger: ILogger, httpDelegate: IHttpDelegate) {
        this.logger = logger;
        this.http = httpDelegate;
    }

    public getConfiguration(): Promise<ExternalConfiguration> {
        return this.http
            .getWithCors<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestConfigService.getConfiguration()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }
}
