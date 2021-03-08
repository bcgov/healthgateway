import { GetterTree } from "vuex";

import MedicationRequest from "@/models/medicationRequest";
import {
    LoadStatus,
    MedicationRequestState,
    RootState,
} from "@/models/storeState";

export const getters: GetterTree<MedicationRequestState, RootState> = {
    medicationRequests(state: MedicationRequestState): MedicationRequest[] {
        return state.medicationRequests;
    },
    medicationCount(state: MedicationRequestState): number {
        return state.medicationRequests.length;
    },
    isLoading(state: MedicationRequestState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
