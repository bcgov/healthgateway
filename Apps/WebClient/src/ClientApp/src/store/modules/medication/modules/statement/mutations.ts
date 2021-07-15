import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";

import {
    MedicationStatementMutations,
    MedicationStatementState,
} from "./types";

export const mutations: MedicationStatementMutations = {
    setMedicationStatementRequested(state: MedicationStatementState) {
        state.status = LoadStatus.REQUESTED;
    },
    setMedicationStatementResult(
        state: MedicationStatementState,
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
    medicationStatementError(state: MedicationStatementState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
