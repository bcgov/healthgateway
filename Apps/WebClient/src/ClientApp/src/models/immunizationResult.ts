import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";

export interface LoadState {
    refreshInProgress: boolean;
}

export default interface ImmunizationResult {
    loadState: LoadState;
    immunizations: ImmunizationEvent[];
    recommendations: Recommendation[];
}
