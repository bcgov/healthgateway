import { LaboratoryOrder } from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryGetters, LaboratoryState } from "./types";

export const getters: LaboratoryGetters = {
    laboratoryOrders(state: LaboratoryState): LaboratoryOrder[] {
        return state.laboratoryOrders;
    },
    laboratoryCount(state: LaboratoryState): number {
        return state.laboratoryOrders.length;
    },
    isLoading(state: LaboratoryState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
