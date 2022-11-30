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

export const notification: NotificationModule = {
    state,
    getters,
    actions,
    mutations,
};
