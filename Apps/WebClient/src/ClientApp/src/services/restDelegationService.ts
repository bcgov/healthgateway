import { ServiceCode } from "@/constants/serviceCodes";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { CreateDelegationRequest } from "@/models/sharing/createDelegationRequest";
import { Action, Actor, Text, Type } from "@/plugins/extensions";
import ErrorTranslator from "@/utility/errorTranslator";

import {
    IDelegationService,
    IHttpDelegate,
    ILogger,
    ITrackingService,
} from "./interfaces";

export class RestDelegationService implements IDelegationService {
    private readonly DELEGATE_BASE_URI: string = "Delegation";
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
    associateDelegation(delegateHdid: string, inviteId: string): Promise<void> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        trackingService.trackEvent({
            action: Action.Submit,
            actor: Actor.Delegate,
            text: Text.Request,
            type: Type.AssociateDelegation,
        });
        return this.http
            .put<void>(
                `${this.baseUri}${this.DELEGATE_BASE_URI}/${delegateHdid}/Associate?encryptedDelegationId=${inviteId}`,
                null
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestDelegationService.associateDelegation()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then(() => {
                this.logger.verbose(`associateDelegation success`);
            });
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
