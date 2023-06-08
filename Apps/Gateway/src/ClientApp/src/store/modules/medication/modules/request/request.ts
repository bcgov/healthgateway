import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { MedicationRequestModule, MedicationRequestState } from "./types";

const state: MedicationRequestState = {
    specialAuthorityRequests: {},
};

export const request: MedicationRequestModule = {
    state,
    getters,
    actions,
    mutations,
};
