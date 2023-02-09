import MedicationRequest from "@/models/medicationRequest";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationRequestGetters, MedicationRequestState } from "./types";

export const getters: MedicationRequestGetters = {
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
