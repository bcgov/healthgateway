import path from "path";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { CreateDelegateInvitationRequest } from "@/models/sharing/createDelegateInvitationRequest";
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
        this.baseUri = config.serviceEndpoints["Delegate"];
    }

    public createInvitation(
        invite: CreateDelegateInvitationRequest
    ): Promise<string | undefined> {
        const uri = path.join(
            this.baseUri,
            this.DELEGATE_BASE_URI,
            "Invitations"
        );
        return this.http
            .post<string>(uri, invite)
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
