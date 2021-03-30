import { MutationTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import MedicationRequest from "@/models/MedicationRequest";
import RequestResult from "@/models/requestResult";
import { LoadStatus, MedicationRequestState } from "@/models/storeState";

export const mutations: MutationTree<MedicationRequestState> = {
    setMedicationRequestRequested(state: MedicationRequestState) {
        state.status = LoadStatus.REQUESTED;
    },

    setMedicationRequestResult(
        state: MedicationRequestState,
        medicationRequestResult: RequestResult<MedicationRequest[]>
    ) {
        if (medicationRequestResult.resultStatus == ResultType.Success) {
            state.medicationRequests = medicationRequestResult.resourcePayload;
            state.statusMessage = "success";
            state.error = undefined;
            state.status = LoadStatus.LOADED;
        } else {
            state.status = LoadStatus.ERROR;
            state.statusMessage =
                "Error returned from the medication requests call";
            state.error = medicationRequestResult.resultError;
        }
    },
    medicationRequestError(state: MedicationRequestState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
