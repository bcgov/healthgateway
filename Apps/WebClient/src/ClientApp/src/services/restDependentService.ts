import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IDependentService,
} from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import Dependent from "@/models/dependent";

@injectable()
export class RestDependentService implements IDependentService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly BASE_URI: string = "/v1/api/Dependent";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getAll(): Promise<RequestResult<Dependent[]>> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<Dependent[]>>(`${this.BASE_URI}/`)
                .then((dependents) => {
                    return resolve(dependents);
                })
                .catch((err) => {
                    this.logger.error(`getAll Dependent error: ${err}`);
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
