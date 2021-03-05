import { MutationTree } from "vuex";

import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { LoadStatus, MedicationState } from "@/models/storeState";

export const mutations: MutationTree<MedicationState> = {
    setRequested(state: MedicationState) {
        state.status = LoadStatus.REQUESTED;
    },
    setMedicationResult(
        state: MedicationState,
        medicationResult: RequestResult<MedicationStatementHistory[]>
    ) {
        if (medicationResult.resultStatus == ResultType.Success) {
            state.protectiveWordAttempts = 0;
            state.medicationStatements = medicationResult.resourcePayload;
            state.statusMessage = "success";
            state.error = undefined;
            state.status = LoadStatus.LOADED;
        } else if (
            medicationResult.resultStatus == ResultType.ActionRequired &&
            medicationResult.resultError?.actionCode == ActionType.Protected
        ) {
            state.protectiveWordAttempts++;
            state.error = undefined;
            state.status = LoadStatus.PROTECTED;
        } else {
            state.status = LoadStatus.ERROR;
            state.statusMessage =
                "Error returned from the medication statements call";
            state.error = medicationResult.resultError;
        }
    },
    medicationError(state: MedicationState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
