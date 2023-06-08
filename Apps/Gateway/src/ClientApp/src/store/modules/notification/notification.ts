import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { NotificationModule, NotificationState } from "./types";

const state: NotificationState = {
    statusMessage: "",
    notifications: [],
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const notification: NotificationModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
