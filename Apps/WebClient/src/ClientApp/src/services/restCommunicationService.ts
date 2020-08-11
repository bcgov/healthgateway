import { injectable } from "inversify";
import { ICommunicationService, IHttpDelegate } from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import Communication from "@/models/communication";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";

@injectable()
export class RestCommunicationService implements ICommunicationService {
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
                    console.log(err);
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
