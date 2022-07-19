import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import AddDependentRequest from "@/models/addDependentRequest";
import { ExternalConfiguration } from "@/models/configData";
import type { Dependent } from "@/models/dependent";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IDependentService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestDependentService implements IDependentService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly DEPENDENT_BASE_URI: string = "UserProfile";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Dependent"];
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public addDependent(
        hdid: string,
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest | undefined> {
        return new Promise<AddDependentRequest | undefined>(
            (resolve, reject) => {
                if (!this.isEnabled) {
                    resolve(undefined);
                    return;
                }
                this.http
                    .post<RequestResult<AddDependentRequest>>(
                        `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent`,
                        dependent
                    )
                    .then((requestResult) => {
                        this.logger.verbose(
                            `addDependent result: ${JSON.stringify(
                                requestResult
                            )}`
                        );
                        return RequestResultUtil.handleResult(
                            requestResult,
                            resolve,
                            reject
                        );
                    })
                    .catch((err: HttpError) => {
                        this.logger.error(
                            `Error in RestDependentService.addDependent()`
                        );
                        return reject(
                            ErrorTranslator.internalNetworkError(
                                err,
                                ServiceCode.HealthGatewayUser
                            )
                        );
                    });
            }
        );
    }

    public getAll(hdid: string): Promise<Dependent[]> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<Dependent[]>>(
                    `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent`
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `getAll dependents result: ${JSON.stringify(
                            requestResult
                        )}`
                    );
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(`Error in RestDependentService.getAll()`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public removeDependent(hdid: string, dependent: Dependent): Promise<void> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<void>>(
                    `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent/${dependent.ownerId}`,
                    dependent
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `removeDependent result: ${JSON.stringify(
                            requestResult
                        )}`
                    );
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestDependentService.removeDependent()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
