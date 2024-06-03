import { ServiceCode } from "@/constants/serviceCodes";
import AddDependentRequest from "@/models/addDependentRequest";
import { ExternalConfiguration } from "@/models/configData";
import type { Dependent } from "@/models/dependent";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import {
    IDependentService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

export class RestDependentService implements IDependentService {
    private readonly DEPENDENT_BASE_URI: string = "UserProfile";
    private logger;
    private http;
    private baseUri;
    private isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
        this.isEnabled =
            config.webClient.featureToggleConfiguration.dependents.enabled;
    }

    public addDependent(
        hdid: string,
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest | undefined> {
        if (!this.isEnabled) {
            return Promise.resolve(undefined);
        }

        return this.http
            .post<RequestResult<AddDependentRequest>>(
                `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent`,
                dependent
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestDependentService.addDependent()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `addDependent result: ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public getAll(hdid: string): Promise<Dependent[]> {
        return this.http
            .getWithCors<RequestResult<Dependent[]>>(
                `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent`
            )
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestDependentService.getAll()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `getAll dependents result: ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public removeDependent(hdid: string, dependent: Dependent): Promise<void> {
        return this.http
            .delete<RequestResult<void>>(
                `${this.baseUri}${this.DEPENDENT_BASE_URI}/${hdid}/Dependent/${dependent.ownerId}`,
                dependent
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestDependentService.removeDependent()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `removeDependent result: ${JSON.stringify(requestResult)}`
                );
                RequestResultUtil.handleResult(requestResult);
            });
    }
}
