import VaccinationIndication from "@/models/vaccinationIndication";

export interface LoadState {
    refreshInProgress: boolean;
}

export default interface VaccinationIndicationResult {
    loadState: LoadState;
    vaccinationIndication: VaccinationIndication;
}
