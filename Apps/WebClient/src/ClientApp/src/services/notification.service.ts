import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { Notification } from "@/models/notification";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface INotificationService {
    getNotifications(hdid: string): Promise<Notification[]>;
    dismissNotification(hdid: string, notificationId: string): Promise<void>;
    dismissNotifications(hdid: string): Promise<void>;
}

/** Port of services/restNotificationService.ts */
export class NotificationService implements INotificationService {
    private readonly NOTIFICATION_BASE_URI = "Notification";
    private readonly baseUri: string;
    private readonly isEnabled: boolean;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.isEnabled =
            config.webClient.featureToggleConfiguration.notificationCentre.enabled;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    async getNotifications(hdid: string): Promise<Notification[]> {
        if (!this.isEnabled) return [];
        try {
            return await this.http.get<Notification[]>(
                `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestNotificationService.getNotifications()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }

    async dismissNotification(
        hdid: string,
        notificationId: string
    ): Promise<void> {
        if (!this.isEnabled) return;
        try {
            await this.http.delete<void>(
                `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}/${notificationId}`,
                notificationId
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestNotificationService.dismissNotification()"
            );
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }

    async dismissNotifications(hdid: string): Promise<void> {
        if (!this.isEnabled) return;
        try {
            await this.http.delete<void>(
                `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}`,
                hdid
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestNotificationService.dismissNotifications()"
            );
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }
}
