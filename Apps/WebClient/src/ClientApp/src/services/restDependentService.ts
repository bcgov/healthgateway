import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IDependentService,
} from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import AddDependentRequest from "@/models/addDependentRequest";
import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import Dependent from "@/models/dependent";

@injectable()
export class RestDependentService implements IDependentService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly BASE_URI: string = "/v1/api";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Dependent"];
    }

    public addDependent(
        hdid: string,
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve();
                return;
            }
            this.http
                .post<RequestResult<AddDependentRequest>>(
                    `${this.BASE_URI}/UserProfile/${hdid}/Dependent`,
                    dependent
                )
                .then((result) => {
                    this.logger.verbose(
                        `addDependent result: ${JSON.stringify(result)}`
                    );
                    return this.handleResult(result, resolve, reject);
                })
                .catch((err) => {
                    this.logger.error(err);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public getAll(hdid: string): Promise<RequestResult<Dependent[]>> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<Dependent[]>>(
                    `${this.BASE_URI}/UserProfile/${hdid}/Dependent`
                )
                .then((dependents) => {
                    return resolve(dependents);
                })
                .catch((err) => {
                    this.logger.error(`getNotes error: ${err}`);
                    return reject(err);
                });
        });
    }

    private handleResult<T>(
        requestResult: RequestResult<T>,
        resolve: (value?: T | PromiseLike<T> | undefined) => void,
        reject: (reason?: unknown) => void
    ) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultError);
        }
    }
}
