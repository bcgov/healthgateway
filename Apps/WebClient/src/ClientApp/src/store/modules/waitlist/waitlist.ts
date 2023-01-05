import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { WaitlistModule, WaitlistState } from "./types";

const state: WaitlistState = {
    status: LoadStatus.NONE,
    tooBusy: false,
    ticket: undefined,
};

const namespaced = true;

export const waitlist: WaitlistModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
