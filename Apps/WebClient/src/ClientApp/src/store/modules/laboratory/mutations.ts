import BannerError from "@/models/bannerError";
import {
    Covid19LaboratoryOrder,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryMutations, LaboratoryState } from "./types";

export const mutations: LaboratoryMutations = {
    setCovid19LaboratoryOrdersRequested(state: LaboratoryState) {
        state.authenticatedCovid19.status = LoadStatus.REQUESTED;
    },
    setCovid19LaboratoryOrders(
        state: LaboratoryState,
        laboratoryOrders: Covid19LaboratoryOrder[]
    ) {
        state.authenticatedCovid19.laboratoryOrders = laboratoryOrders;
        state.authenticatedCovid19.error = undefined;
        state.authenticatedCovid19.statusMessage = "success";
        state.authenticatedCovid19.status = LoadStatus.LOADED;
    },
    covid19LaboratoryError(state: LaboratoryState, error: Error) {
        state.authenticatedCovid19.statusMessage = error.message;
        state.authenticatedCovid19.status = LoadStatus.ERROR;
    },
    setPublicCovidTestResponseResultRequested(state: LaboratoryState) {
        state.publicCovid19.publicCovidTestResponseResult = undefined;
        state.publicCovid19.status = LoadStatus.REQUESTED;
        state.publicCovid19.statusMessage = "";
        state.publicCovid19.error = undefined;
    },
    setPublicCovidTestResponseResult(
        state: LaboratoryState,
        publicCovidTestResponseResult: PublicCovidTestResponseResult
    ) {
        state.publicCovid19.publicCovidTestResponseResult =
            publicCovidTestResponseResult;
        state.publicCovid19.status = LoadStatus.LOADED;
        state.publicCovid19.statusMessage = "";
        state.publicCovid19.error = undefined;
    },
    setPublicCovidTestResponseResultError(
        state: LaboratoryState,
        error: BannerError
    ) {
        state.publicCovid19.error = error;
        state.publicCovid19.status = LoadStatus.ERROR;
    },
    setPublicCovidTestResponseResultStatusMessage(
        state: LaboratoryState,
        statusMessage: string
    ) {
        state.publicCovid19.statusMessage = statusMessage;
    },
    resetPublicCovidTestResponseResult(state: LaboratoryState) {
        state.publicCovid19 = {
            publicCovidTestResponseResult: undefined,
            status: undefined,
            statusMessage: "",
            error: undefined,
        };
    },
};
