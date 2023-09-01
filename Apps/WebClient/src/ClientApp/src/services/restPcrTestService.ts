import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import RequestResult from "@/models/requestResult";
import { IHttpDelegate, ILogger, IPcrTestService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

export class RestPcrTestService implements IPcrTestService {
    private readonly LABORATORY_BASE_URI: string = "Laboratory";
    private readonly PUBLIC_LABORATORY_BASE_URI: string = "PublicLaboratory";
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
        this.baseUri = config.serviceEndpoints["Laboratory"];
        this.isEnabled =
            config.webClient.featureToggleConfiguration.covid19.pcrTestEnabled;
    }

    public registerTestKit(
        hdid: string,
        testKit: RegisterTestKitRequest
    ): Promise<RegisterTestKitRequest | undefined> {
        if (!this.isEnabled) {
            return Promise.resolve(undefined);
        }

        return this.http
            .post<RequestResult<RegisterTestKitRequest>>(
                `${this.baseUri}${this.LABORATORY_BASE_URI}/${hdid}/LabTestKit`,
                testKit
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestPcrTestService.registerTestKit()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `registerTestKit result: ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public registerTestKitPublic(
        testKit: RegisterTestKitPublicRequest
    ): Promise<RegisterTestKitPublicRequest | undefined> {
        if (!this.isEnabled) {
            return Promise.resolve(undefined);
        }

        return this.http
            .post<RequestResult<RegisterTestKitPublicRequest>>(
                `${this.baseUri}${this.PUBLIC_LABORATORY_BASE_URI}/LabTestKit`,
                testKit
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestPcrTestService.registerTestKitPublic()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `registerTestKitPublic result: ${JSON.stringify(
                        requestResult
                    )}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }
}
