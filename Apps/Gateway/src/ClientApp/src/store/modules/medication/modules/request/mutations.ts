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
    setSpecialAuthorityRequestsRequested(
        state: MedicationRequestState,
        hdid: string
    ) {
        const currentState = getSpecialAuthorityRequestState(state, hdid);
        const nextState: SpecialAuthorityRequestState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setSpecialAuthorityRequestState(state, hdid, nextState);
    },

    setSpecialAuthorityRequests(
        state: MedicationRequestState,
        payload: {
            hdid: string;
            specialAuthorityRequestsResult: RequestResult<MedicationRequest[]>;
        }
    ) {
        const { hdid, specialAuthorityRequestsResult } = payload;
        const currentState = getSpecialAuthorityRequestState(state, hdid);

        const nextState: SpecialAuthorityRequestState = {
            ...currentState,
        };

        if (specialAuthorityRequestsResult.resultStatus == ResultType.Success) {
            nextState.data = specialAuthorityRequestsResult.resourcePayload;
            nextState.statusMessage = "success";
            nextState.error = undefined;
            nextState.status = LoadStatus.LOADED;
        } else {
            nextState.status = LoadStatus.ERROR;
            nextState.statusMessage =
                "Error returned from the Special Authority requests call";
            nextState.error = specialAuthorityRequestsResult.resultError;
        }

        setSpecialAuthorityRequestState(state, hdid, nextState);
    },
    setSpecialAuthorityRequestsError(
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
