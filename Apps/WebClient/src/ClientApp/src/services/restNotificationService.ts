import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import Notification from "@/models/notification";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

import { INotificationService } from "./interfaces";

@injectable()
export class RestNotificationService implements INotificationService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly NOTIFICATION_BASE_URI: string = "Notification";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    getNotifications(hdid: string): Promise<Notification[]> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<Notification[]>(
                    `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}`
                )
                .then((result) => resolve(result))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestNotificationService.getNotifications()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }
    dismissNotification(hdid: string, notificationId: string): Promise<void> {
        return new Promise((resolve, reject) =>
            this.http
                .delete<void>(
                    `${this.baseUri}${this.NOTIFICATION_BASE_URI}${hdid}/notificationId`,
                    notificationId
                )
                .then((result) => {
                    this.logger.debug(`dismissNotification ${result}`);
                    resolve();
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestNotificationService.dismissNotification()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
    dismissNotifications(hdid: string): Promise<void> {
        return new Promise((resolve, reject) =>
            this.http
                .delete<void>(
                    `${this.baseUri}${this.NOTIFICATION_BASE_URI}${hdid}`,
                    hdid
                )
                .then((result) => {
                    this.logger.debug(`dismissNotifications ${result}`);
                    resolve();
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestNotificationService.dismissNotifications()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
}
