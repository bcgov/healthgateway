import BannerError from "@/models/bannerError";
import {
    LaboratoryOrder,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryMutations, LaboratoryState } from "./types";

const initialPublicState = {
    publicCovidTestResponseResult: undefined,
    status: undefined,
    statusMessage: "",
    error: undefined,
};

export const mutations: LaboratoryMutations = {
    setRequested(state: LaboratoryState) {
        state.authenticated.status = LoadStatus.REQUESTED;
    },
    setLaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrders: LaboratoryOrder[]
    ) {
        state.authenticated.laboratoryOrders = laboratoryOrders;
        state.authenticated.error = undefined;
        state.authenticated.statusMessage = "success";
        state.authenticated.status = LoadStatus.LOADED;
    },
    laboratoryError(state: LaboratoryState, error: Error) {
        state.authenticated.statusMessage = error.message;
        state.authenticated.status = LoadStatus.ERROR;
    },
    setPublicCovidTestResponseResultRequested(state: LaboratoryState) {
        state.public.publicCovidTestResponseResult = undefined;
        state.public.status = LoadStatus.REQUESTED;
        state.public.statusMessage = "";
        state.public.error = undefined;
    },
    setPublicCovidTestResponseResult(
        state: LaboratoryState,
        publicCovidTestResponseResult: PublicCovidTestResponseResult
    ) {
        state.public.publicCovidTestResponseResult =
            publicCovidTestResponseResult;
        state.public.status = LoadStatus.LOADED;
        state.public.statusMessage = "";
        state.public.error = undefined;
    },
    setPublicCovidTestResponseResultError(
        state: LaboratoryState,
        error: BannerError
    ) {
        state.public.error = error;
        state.public.status = LoadStatus.ERROR;
    },
    setPublicCovidTestResponseResultStatusMessage(
        state: LaboratoryState,
        statusMessage: string
    ) {
        state.public.statusMessage = statusMessage;
    },
    resetPublicCovidTestResponseResult(state: LaboratoryState) {
        Object.assign(state.public, initialPublicState);
    },
};
