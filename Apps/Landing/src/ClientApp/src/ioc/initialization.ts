import { container } from "@/ioc/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { HttpDelegate } from "@/services/httpDelegate";
import {
    ICommunicationService,
    IConfigService,
    IHttpDelegate,
    ILogger,
    IUserProfileService,
} from "@/services/interfaces";
import { LoglevelLogger } from "@/services/loglevelLogger";
import { RestCommunicationService } from "@/services/restCommunicationService";
import { RestConfigService } from "@/services/restConfigService";
import { RestUserProfileService } from "@/services/restUserProfileService";
import { useConfigStore } from "@/stores/config";

export async function initializeServices(): Promise<void> {
    const configStore = useConfigStore();

    container.set<ILogger>(
        SERVICE_IDENTIFIER.Logger,
        () => new LoglevelLogger(configStore.webConfig?.logLevel?.toLowerCase())
    );

    container.set<IHttpDelegate>(
        DELEGATE_IDENTIFIER.HttpDelegate,
        (c) => new HttpDelegate(c.get<ILogger>(SERVICE_IDENTIFIER.Logger))
    );

    container.set<IConfigService>(
        SERVICE_IDENTIFIER.ConfigService,
        (c) =>
            new RestConfigService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
            )
    );

    container.set<ICommunicationService>(
        SERVICE_IDENTIFIER.CommunicationService,
        (c) =>
            new RestCommunicationService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IUserProfileService>(
        SERVICE_IDENTIFIER.UserProfileService,
        (c) =>
            new RestUserProfileService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );
}
