import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import Notification from "@/models/notification";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, INotificationService } from "@/services/interfaces";

import { NotificationActions } from "./types";

export const actions: NotificationActions = {
    retrieve(context): Promise<Notification[]> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const notificationService = container.get<INotificationService>(
            SERVICE_IDENTIFIER.NotificationService
        );

        const user: User = context.rootGetters["user/user"];

        return new Promise((resolve, reject) => {
            const notifications: Notification[] = context.getters.notifications;
            if (context.state.status === LoadStatus.LOADED) {
                logger.debug(`Notifications found stored, not querying!`);
                resolve(notifications);
            } else {
                logger.debug(`Retrieving Notifications`);
                context.commit("setRequested");
                notificationService
                    .getNotifications(user.hdid)
                    .then((result) => {
                        if (result) {
                            context.commit("setNotifications", result);
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
        params: { notificationId: string }
    ): Promise<void> {
        const notificationService = container.get<INotificationService>(
            SERVICE_IDENTIFIER.NotificationService
        );

        const user: User = context.rootGetters["user/user"];

        return new Promise((resolve, reject) =>
            notificationService
                .dismissNotification(user.hdid, params.notificationId)
                .then(() => {
                    context.commit(
                        "dismissNotification",
                        params.notificationId
                    );
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
    dismissAllNotifications(context): Promise<void> {
        const notificationService = container.get<INotificationService>(
            SERVICE_IDENTIFIER.NotificationService
        );

        const user: User = context.rootGetters["user/user"];

        return new Promise((resolve, reject) =>
            notificationService
                .dismissNotifications(user.hdid)
                .then(() => {
                    context.commit("dismissAllNotifications");
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
