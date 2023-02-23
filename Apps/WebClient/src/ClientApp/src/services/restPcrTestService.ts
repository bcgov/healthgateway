import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger, IPcrTestService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestPcrTestService implements IPcrTestService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    private readonly LABORATORY_BASE_URI: string = "Laboratory";
    private readonly PUBLIC_LABORATORY_BASE_URI: string = "PublicLaboratory";

    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Laboratory"];
        this.http = http;
        this.isEnabled =
            config.webClient.featureToggleConfiguration.covid19.pcrTestEnabled;
    }

    public registerTestKit(
        hdid: string,
        testKit: RegisterTestKitRequest
    ): Promise<RegisterTestKitRequest | undefined> {
        return new Promise<RegisterTestKitRequest | undefined>(
            (resolve, reject) => {
                if (!this.isEnabled) {
                    resolve(undefined);
                    return;
                }
                this.http
                    .post<RequestResult<RegisterTestKitRequest>>(
                        `${this.baseUri}${this.LABORATORY_BASE_URI}/${hdid}/LabTestKit`,
                        testKit
                    )
                    .then((requestResult) => {
                        this.logger.verbose(
                            `registerTestKit result: ${JSON.stringify(
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
                            `Error in RestPcrTestService.registerTestKit()`
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

    public registerTestKitPublic(
        testKit: RegisterTestKitPublicRequest
    ): Promise<RegisterTestKitPublicRequest | undefined> {
        return new Promise<RegisterTestKitPublicRequest | undefined>(
            (resolve, reject) => {
                if (!this.isEnabled) {
                    resolve(undefined);
                    return;
                }
                this.http
                    .post<RequestResult<RegisterTestKitPublicRequest>>(
                        `${this.baseUri}${this.PUBLIC_LABORATORY_BASE_URI}/LabTestKit`,
                        testKit
                    )
                    .then((requestResult) => {
                        this.logger.verbose(
                            `registerTestKitPublic result: ${JSON.stringify(
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
                            `Error in RestPcrTestService.registerTestKitPublic()`
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
}
