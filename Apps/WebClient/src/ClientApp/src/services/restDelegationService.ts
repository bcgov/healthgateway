import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { CreateDelegationRequest } from "@/models/sharing/createDelegationRequest";
import ErrorTranslator from "@/utility/errorTranslator";

import { IDelegationService, IHttpDelegate, ILogger } from "./interfaces";

export class RestDelegationService implements IDelegationService {
    private readonly DELEGATE_BASE_URI: string = "Delegate";
    private logger;
    private http;
    private baseUri;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public createDelegation(
        ownerHdid: string,
        delegation: CreateDelegationRequest
    ): Promise<string | undefined> {
        return this.http
            .post<string>(
                `${this.baseUri}${this.DELEGATE_BASE_URI}/${ownerHdid}`,
                delegation
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestDelegationService.createInvitation()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((sharingCode) => {
                this.logger.verbose(
                    `createDelegation sharing code: ${sharingCode}`
                );
                return sharingCode;
            });
    }
}
