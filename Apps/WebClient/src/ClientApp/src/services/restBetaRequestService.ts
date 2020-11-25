import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IBetaRequestService,
    IHttpDelegate,
} from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import BetaRequest from "@/models/betaRequest";
import RequestResult from "@/models/requestResult";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestBetaRequestService implements IBetaRequestService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly BETA_REQUEST_BASE_URI: string = "/v1/api/BetaRequest";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getRequested(hdid: string): Promise<BetaRequest> {
        return new Promise((resolve, reject) => {
            this.http
                .get<BetaRequest>(`${this.BETA_REQUEST_BASE_URI}/${hdid}`)
                .then((betaRequest) => {
                    return resolve(betaRequest);
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

    public putRequest(
        hdid: string,
        request: BetaRequest
    ): Promise<BetaRequest> {
        return new Promise<BetaRequest>((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";
            this.http
                .put<RequestResult<BetaRequest>>(
                    `${this.BETA_REQUEST_BASE_URI}/${hdid}`,
                    JSON.stringify(request),
                    headers
                )
                .then((requestResult) => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
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
}
