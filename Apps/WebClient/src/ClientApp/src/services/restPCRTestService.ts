import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
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
    private readonly BASE_URI: string = "/v1/api";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["PCRTest"];
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
                        `${this.BASE_URI}/Public/LabTestKit/Registration`,
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
                        this.logger.error(err);
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
