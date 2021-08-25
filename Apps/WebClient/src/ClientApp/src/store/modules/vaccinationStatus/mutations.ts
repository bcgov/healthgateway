import BannerError from "@/models/bannerError";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";

import { VaccinationStatusMutations, VaccinationStatusState } from "./types";

export const mutations: VaccinationStatusMutations = {
    setRequested(state: VaccinationStatusState) {
        state.error = undefined;
        state.status =
            state.status === LoadStatus.DEFERRED
                ? LoadStatus.ASYNC_REQUESTED
                : LoadStatus.REQUESTED;
        state.statusMessage = "";
    },
    setVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ) {
        state.vaccinationStatus = vaccinationStatus;
        state.status = LoadStatus.LOADED;
        state.statusMessage = "";
    },
    vaccinationStatusError(state: VaccinationStatusState, error: BannerError) {
        state.vaccinationStatus = undefined;
        state.error = error;
        state.status = LoadStatus.ERROR;
        state.statusMessage = "";
    },
    setStatusMessage(state: VaccinationStatusState, statusMessage: string) {
        state.statusMessage = statusMessage;
    },
};
