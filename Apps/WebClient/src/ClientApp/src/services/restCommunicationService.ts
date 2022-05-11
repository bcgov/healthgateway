import { injectable } from "inversify";

import Communication, { CommunicationType } from "@/models/communication";
import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ICommunicationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestCommunicationService implements ICommunicationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly BASE_URI: string = "v1/api/Communication";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public getActive(
        type: CommunicationType
    ): Promise<RequestResult<Communication>> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<Communication>>(
                    `${this.baseUri}${this.BASE_URI}/${type}`
                )
                .then((communication) => {
                    return resolve(communication);
                })
                .catch((err) => {
                    this.logger.error(`getActive Communication error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
