import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IHttpDelegate, ILogger, IPCRTestService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestPCRTestService implements IPCRTestService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

    private readonly LABORATORY_BASE_URI: string = "v1/api/Laboratory";
    private readonly PUBLIC_LABORATORY_BASE_URI: string =
        "v1/api/PublicLaboratory";

    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["PcrTest"];
        this.baseUri = config.serviceEndpoints["Laboratory"];
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
                    .catch((err) => {
                        this.logger.error("registerTestKit Error: " + err);
                        return reject(
                            ErrorTranslator.internalNetworkError(
                                err,
                                ServiceName.HealthGatewayUser
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
                    .catch((err) => {
                        this.logger.error(
                            "registerTestKitPublic error: " + err
                        );
                        return reject(
                            ErrorTranslator.internalNetworkError(
                                err,
                                ServiceName.HealthGatewayUser
                            )
                        );
                    });
            }
        );
    }
}
