import MedicationRequest from "@/models/medicationRequest";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationRequestGetters, MedicationRequestState } from "./types";
import { getSpecialAuthorityRequestState } from "./util";

export const getters: MedicationRequestGetters = {
    medicationRequests(
        state: MedicationRequestState
    ): (hdid: string) => MedicationRequest[] {
        return (hdid: string) =>
            getSpecialAuthorityRequestState(state, hdid).data;
    },
    medicationRequestCount(
        state: MedicationRequestState
    ): (hdid: string) => number {
        return (hdid: string) =>
            getSpecialAuthorityRequestState(state, hdid).data.length;
    },
    isMedicationRequestLoading(
        state: MedicationRequestState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getSpecialAuthorityRequestState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
};
