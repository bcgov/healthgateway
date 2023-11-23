import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { CreateDelegationRequest } from "@/models/sharing/createDelegationRequest";
import ErrorTranslator from "@/utility/errorTranslator";

import { IDelegateService, IHttpDelegate, ILogger } from "./interfaces";

export class RestDelegateService implements IDelegateService {
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

    public createInvitation(
        ownerHdid: string,
        invite: CreateDelegationRequest
    ): Promise<string | undefined> {
        return this.http
            .post<string>(
                `${this.baseUri}${this.DELEGATE_BASE_URI}/${ownerHdid}`,
                invite
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestDelegateService.createInvitation()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((sharingCode) => {
                this.logger.verbose(
                    `createInvitation sharing code: ${sharingCode}`
                );
                return sharingCode;
            });
    }
}
