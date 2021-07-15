import { LaboratoryOrder } from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryMutations, LaboratoryState } from "./types";

export const mutations: LaboratoryMutations = {
    setRequested(state: LaboratoryState) {
        state.status = LoadStatus.REQUESTED;
    },
    setLaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrders: LaboratoryOrder[]
    ) {
        state.laboratoryOrders = laboratoryOrders;
        state.error = undefined;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    laboratoryError(state: LaboratoryState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
