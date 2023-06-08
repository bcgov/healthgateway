import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { MedicationStatementModule, MedicationStatementState } from "./types";

const state: MedicationStatementState = {
    medications: {},
};

export const statement: MedicationStatementModule = {
    state,
    getters,
    actions,
    mutations,
};
