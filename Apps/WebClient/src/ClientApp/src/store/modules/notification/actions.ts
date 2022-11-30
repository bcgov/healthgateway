import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import ApiResult from "@/models/apiResult";
import { ResultError } from "@/models/errors";
import Notification from "@/models/notification";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, INotificationService } from "@/services/interfaces";

import { NotificationActions } from "./types";

export const actions: NotificationActions = {
    retrieve(
        context,
        params: { hdid: string }
    ): Promise<ApiResult<Notification[]>> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const notificationService = container.get<INotificationService>(
            SERVICE_IDENTIFIER.NotificationService
        );

        return new Promise((resolve, reject) => {
            const notifications: Notification[] = context.getters.notification;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Notifications found stored, not querying!`);
                resolve({
                    resourcePayload: notifications,
                });
            } else {
                logger.debug(`Retrieving Notifications`);
                context.commit("setRequested");
                notificationService
                    .getNotifications(params.hdid)
                    .then((result) => {
                        if (result.resourcePayload) {
                            context.commit(
                                "setNotifications",
                                result.resourcePayload
                            );
                            resolve(result);
                        }
                    })
                    .catch((error: ResultError) => {
                        context.dispatch("handleError", {
                            error,
                            errorType: ErrorType.Retrieve,
                        });
                        reject(error);
                    });
            }
        });
    },
    dismissNotification(
        context,
        params: { hdid: string; notification: Notification }
    ): Promise<void> {
        const notificationService = container.get<INotificationService>(
            SERVICE_IDENTIFIER.NotificationService
        );

        return new Promise((resolve, reject) =>
            notificationService
                .dismissNotification(params.hdid, params.notification)
                .then(() => {
                    context.commit("dismissNotification", params.notification);
                    resolve();
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Delete,
                    });
                    reject(error);
                })
        );
    },
    dismissNotifications(context, params: { hdid: string }): Promise<void> {
        const notificationService = container.get<INotificationService>(
            SERVICE_IDENTIFIER.NotificationService
        );

        return new Promise((resolve, reject) =>
            notificationService
                .dismissNotifications(params.hdid)
                .then(() => {
                    context.commit("dismissNotifications", params.hdid);
                    resolve();
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Delete,
                    });
                    reject(error);
                })
        );
    },
    handleError(context, params: { error: ResultError; errorType: ErrorType }) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("notificationError", params.error);

        if (params.error.statusCode === 429) {
            if (params.errorType === ErrorType.Retrieve) {
                context.dispatch(
                    "errorBanner/setTooManyRequestsWarning",
                    { key: "page" },
                    { root: true }
                );
            } else {
                context.dispatch(
                    "errorBanner/setTooManyRequestsError",
                    { key: "page" },
                    { root: true }
                );
            }
        } else {
            context.dispatch(
                "errorBanner/addError",
                {
                    errorType: params.errorType,
                    source: ErrorSourceType.Notification,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
};
