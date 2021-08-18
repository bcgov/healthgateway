import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";

import { VaccinationStatusMutations, VaccinationStatusState } from "./types";

export const mutations: VaccinationStatusMutations = {
    setRequested(state: VaccinationStatusState) {
        state.status =
            state.status === LoadStatus.DEFERRED
                ? LoadStatus.ASYNC_REQUESTED
                : LoadStatus.REQUESTED;
    },
    setVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ) {
        state.vaccinationStatus = vaccinationStatus;
        state.error = undefined;
        state.status = LoadStatus.LOADED;
    },
    vaccinationStatusError(state: VaccinationStatusState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
