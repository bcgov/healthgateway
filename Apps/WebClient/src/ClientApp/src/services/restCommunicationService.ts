import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    ICommunicationService,
    IHttpDelegate,
} from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import Communication from "@/models/communication";

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
                    return reject(err);
                });
        });
    }
}
