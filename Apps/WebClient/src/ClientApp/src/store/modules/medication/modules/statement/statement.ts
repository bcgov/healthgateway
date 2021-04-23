import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { MedicationStatementModule, MedicationStatementState } from "./types";

const state: MedicationStatementState = {
    medicationStatements: [],
    protectiveWordAttempts: 0,
    status: LoadStatus.NONE,
    error: undefined,
    statusMessage: "",
};

export const statement: MedicationStatementModule = {
    state,
    getters,
    actions,
    mutations,
};
