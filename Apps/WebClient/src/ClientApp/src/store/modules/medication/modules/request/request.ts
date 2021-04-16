import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { MedicationRequestModule, MedicationRequestState } from "./types";

const state: MedicationRequestState = {
    medicationRequests: [],
    status: LoadStatus.NONE,
    error: undefined,
    statusMessage: "",
};

export const request: MedicationRequestModule = {
    state,
    getters,
    actions,
    mutations,
};
