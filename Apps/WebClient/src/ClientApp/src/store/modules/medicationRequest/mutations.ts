import { MutationTree } from "vuex";

import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import MedicationRequest from "@/models/MedicationRequest";
import RequestResult from "@/models/requestResult";
import { LoadStatus, MedicationRequestState } from "@/models/storeState";

export const mutations: MutationTree<MedicationRequestState> = {
    setRequested(state: MedicationRequestState) {
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
        } else if (
            medicationRequestResult.resultStatus == ResultType.ActionRequired &&
            medicationRequestResult.resultError?.actionCode ==
                ActionType.Protected
        ) {
            state.error = undefined;
            state.status = LoadStatus.PROTECTED;
        } else {
            state.status = LoadStatus.ERROR;
            state.statusMessage =
                "Error returned from the medication statements call";
            state.error = medicationRequestResult.resultError;
        }
    },
    medicationRequestError(state: MedicationRequestState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
