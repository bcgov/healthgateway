import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import ApiResult from "@/models/apiResult";
import { ResultError } from "@/models/errors";
import Notification from "@/models/notification";
import { LoadStatus, Operation } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface NotificationState {
    notifications: Notification[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
    lastOperation: Operation | null;
}

export interface NotificationGetters
    extends GetterTree<NotificationState, RootState> {
    notifications(state: NotificationState): Notification[];
}

type StoreContext = ActionContext<NotificationState, RootState>;
export interface NotificationActions
    extends ActionTree<NotificationState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<ApiResult<Notification[]>>;
    dismissNotification(
        context: StoreContext,
        params: { hdid: string; notification: Notification }
    ): Promise<void>;
    dismissNotifications(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<void>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface NotificationMutations extends MutationTree<NotificationState> {
    dismissNotification(
        state: NotificationState,
        notification: Notification
    ): void;
    dismissNotifications(state: NotificationState): void;
    notificationError(state: NotificationState, error: ResultError): void;
}

export interface NotificationModule
    extends Module<NotificationState, RootState> {
    state: NotificationState;
    getters: NotificationGetters;
    actions: NotificationActions;
    mutations: NotificationMutations;
}
