import { GetterTree } from "vuex";

import { LaboratoryOrder } from "@/models/laboratory";
import { LaboratoryState, RootState } from "@/models/storeState";

export const getters: GetterTree<LaboratoryState, RootState> = {
    getStoredLaboratoryOrders: (
        state: LaboratoryState
    ) => (): LaboratoryOrder[] => {
        return state.laboratoryOrders;
    },
};
