import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";

import { VaccinationStatusGetters, VaccinationStatusState } from "./types";

export const getters: VaccinationStatusGetters = {
    vaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined {
        return state.vaccinationStatus;
    },
    isLoading(state: VaccinationStatusState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
