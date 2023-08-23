import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { IConfigService, IHttpDelegate, ILogger } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestConfigService implements IConfigService {
    private readonly CONFIG_BASE_URI: string = "/configuration";
    private logger;
    private http;

    constructor(logger: ILogger, httpDelegate: IHttpDelegate) {
        this.logger = logger;
        this.http = httpDelegate;
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
