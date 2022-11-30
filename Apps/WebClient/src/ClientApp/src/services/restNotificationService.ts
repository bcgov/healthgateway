import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import ApiResult from "@/models/apiResult";
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
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Notification"];
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    getNotifications(hdid: string): Promise<ApiResult<Notification[]>> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<ApiResult<Notification[]>>(
                    `${this.baseUri}${this.NOTIFICATION_BASE_URI}/${hdid}`
                )
                .then((apiResult) => resolve(apiResult))
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
    dismissNotification(
        hdid: string,
        notification: Notification
    ): Promise<void> {
        return new Promise((resolve, reject) =>
            this.http
                .delete<ApiResult<void>>(
                    `${this.baseUri}${this.NOTIFICATION_BASE_URI}${hdid}/notificationId`,
                    notification.id
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
                .delete<ApiResult<void>>(
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
