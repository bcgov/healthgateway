import { GetterTree } from "vuex";

import MedicationRequest from "@/models/MedicationRequest";
import {
    LoadStatus,
    MedicationRequestState,
    RootState,
} from "@/models/storeState";

export const getters: GetterTree<MedicationRequestState, RootState> = {
    medicationRequests(state: MedicationRequestState): MedicationRequest[] {
        return state.medicationRequests;
    },
    medicationRequestCount(state: MedicationRequestState): number {
        return state.medicationRequests.length;
    },
    isMedicationRequestLoading(state: MedicationRequestState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
