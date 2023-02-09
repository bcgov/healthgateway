import { ResultType } from "@/constants/resulttype";
import { SpecialAuthorityRequestState } from "@/models/datasetState";
import MedicationRequest from "@/models/medicationRequest";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationRequestMutations, MedicationRequestState } from "./types";
import {
    getSpecialAuthorityRequestState,
    setSpecialAuthorityRequestState,
} from "./util";

export const mutations: MedicationRequestMutations = {
    setMedicationRequestRequested(state: MedicationRequestState, hdid: string) {
        const currentState = getSpecialAuthorityRequestState(state, hdid);
        const nextState: SpecialAuthorityRequestState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setSpecialAuthorityRequestState(state, hdid, nextState);
    },

    setMedicationRequestResult(
        state: MedicationRequestState,
        payload: {
            hdid: string;
            medicationRequestResult: RequestResult<MedicationRequest[]>;
        }
    ) {
        const { hdid, medicationRequestResult } = payload;
        const currentState = getSpecialAuthorityRequestState(state, hdid);

        const nextState: SpecialAuthorityRequestState = {
            ...currentState,
        };

        if (medicationRequestResult.resultStatus == ResultType.Success) {
            nextState.data = medicationRequestResult.resourcePayload;
            nextState.statusMessage = "success";
            nextState.error = undefined;
            nextState.status = LoadStatus.LOADED;
        } else {
            nextState.status = LoadStatus.ERROR;
            nextState.statusMessage =
                "Error returned from the medication requests call";
            nextState.error = medicationRequestResult.resultError;
        }

        setSpecialAuthorityRequestState(state, hdid, nextState);
    },
    medicationRequestError(
        state: MedicationRequestState,
        payload: { hdid: string; error: Error }
    ) {
        const { hdid, error } = payload;
        const currentState = getSpecialAuthorityRequestState(state, hdid);
        const nextState: SpecialAuthorityRequestState = {
            ...currentState,
            statusMessage: error.message,
            status: LoadStatus.ERROR,
        };
        setSpecialAuthorityRequestState(state, hdid, nextState);
    },
};
