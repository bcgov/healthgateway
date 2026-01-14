import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import Notification from "@/models/notification";
import {
    IHttpDelegate,
    ILogger,
    INotificationService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestNotificationService implements INotificationService {
    private readonly NOTIFICATION_BASE_URI: string = "Notification";
    private readonly logger;
    private readonly http;
    private readonly baseUri;
    private readonly isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.isEnabled =
            config.webClient.featureToggleConfiguration.notificationCentre.enabled;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public getNotifications(hdid: string): Promise<Notification[]> {
        if (!this.isEnabled) {
            return Promise.resolve([]);
        }

        return this.http
            .getWithCors<
                Notification[]
            >(`${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestNotificationService.getNotifications()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public dismissNotification(
        hdid: string,
        notificationId: string
    ): Promise<void> {
        if (!this.isEnabled) {
            return Promise.resolve();
        }

        return this.http
            .delete<void>(
                `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}/${notificationId}`,
                notificationId
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestNotificationService.dismissNotification()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public dismissNotifications(hdid: string): Promise<void> {
        if (!this.isEnabled) {
            return Promise.resolve();
        }

        return this.http
            .delete<void>(
                `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}`,
                hdid
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestNotificationService.dismissNotifications()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }
}
