import { GetterTree } from "vuex";

import { LaboratoryOrder } from "@/models/laboratory";
import { LaboratoryState, LoadStatus, RootState } from "@/models/storeState";

export const getters: GetterTree<LaboratoryState, RootState> = {
    laboratoryOrders(state: LaboratoryState): LaboratoryOrder[] {
        return state.laboratoryOrders;
    },
    isLoading(state: LaboratoryState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
