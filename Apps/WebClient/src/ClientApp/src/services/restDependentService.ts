import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IDependentService,
} from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import type { AddDependentRequest } from "@/models/addDependentRequest";
import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import { Dependent } from "@/models/dependent";

@injectable()
export class RestDependentService implements IDependentService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly DEPENDENT_BASE_URI: string = "/v1/api/Dependent";
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
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve();
                return;
            }
            this.http
                .post<RequestResult<AddDependentRequest>>(
                    `${this.DEPENDENT_BASE_URI}/`,
                    dependent
                )
                .then((result) => {
                    this.logger.verbose(
                        `createComment result: ${JSON.stringify(result)}`
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

    public getAllDependents(): Promise<RequestResult<Dependent[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: [],
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }

            this.http
                .getWithCors<RequestResult<Dependent[]>>(
                    `${this.DEPENDENT_BASE_URI}/`
                )
                .then((requestResult) => {
                    return resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`getNotes error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
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
