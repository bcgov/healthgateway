import { MutationTree } from "vuex";

import { LaboratoryOrder } from "@/models/laboratory";
import { LaboratoryState, LoadStatus } from "@/models/storeState";

export const mutations: MutationTree<LaboratoryState> = {
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
