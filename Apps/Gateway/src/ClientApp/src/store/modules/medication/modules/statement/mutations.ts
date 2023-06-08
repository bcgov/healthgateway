import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import { MedicationState } from "@/models/datasetState";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";

import {
    MedicationStatementMutations,
    MedicationStatementState,
} from "./types";
import { getMedicationState, setMedicationState } from "./util";

export const mutations: MedicationStatementMutations = {
    setMedicationsRequested(state: MedicationStatementState, hdid: string) {
        const currentState = getMedicationState(state, hdid);
        const nextState: MedicationState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setMedicationState(state, hdid, nextState);
    },
    setMedications(
        state: MedicationStatementState,
        payload: {
            hdid: string;
            medicationResult: RequestResult<MedicationStatementHistory[]>;
        }
    ) {
        const { hdid, medicationResult } = payload;
        const currentState = getMedicationState(state, hdid);

        const nextState: MedicationState = {
            ...currentState,
        };

        if (medicationResult.resultStatus == ResultType.Success) {
            nextState.protectiveWordAttempts = 0;
            nextState.data = medicationResult.resourcePayload;
            nextState.statusMessage = "success";
            nextState.error = undefined;
            nextState.status = LoadStatus.LOADED;
        } else if (
            medicationResult.resultStatus == ResultType.ActionRequired &&
            medicationResult.resultError?.actionCode == ActionType.Protected
        ) {
            nextState.protectiveWordAttempts++;
            nextState.error = undefined;
            nextState.status = LoadStatus.PROTECTED;
        } else {
            nextState.status = LoadStatus.ERROR;
            nextState.statusMessage =
                "Error returned from the medications call";
            nextState.error = medicationResult.resultError;
        }

        setMedicationState(state, hdid, nextState);
    },
    setMedicationsError(
        state: MedicationStatementState,
        payload: { hdid: string; error: Error }
    ) {
        const { hdid, error } = payload;
        const currentState = getMedicationState(state, hdid);
        const nextState: MedicationState = {
            ...currentState,
            statusMessage: error.message,
            status: LoadStatus.ERROR,
        };
        setMedicationState(state, hdid, nextState);
    },
};
