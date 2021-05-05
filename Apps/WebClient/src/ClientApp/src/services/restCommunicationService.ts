import { injectable } from "inversify";

import Communication from "@/models/communication";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    ICommunicationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestCommunicationService implements ICommunicationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly BASE_URI: string = "/v1/api/Communication";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getActive(): Promise<RequestResult<Communication>> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<Communication>>(`${this.BASE_URI}/`)
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
