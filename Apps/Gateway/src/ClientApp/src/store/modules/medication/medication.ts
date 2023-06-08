import { LoadStatus } from "@/models/storeOperations";

import { request } from "./modules/request/request";
import { statement } from "./modules/statement/statement";
import { MedicationModule, MedicationState } from "./types";

const state: MedicationState = {
    status: LoadStatus.NONE,
};

const namespaced = true;

export const medication: MedicationModule = {
    namespaced,
    state,
    modules: { statement, request },
};
