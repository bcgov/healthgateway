import ImmunizationModel from "@/models/immunizationModel";

export default interface PhsaLoadState {
    refreshInProgress: boolean;
}

export default interface ImmunizationResult {
    loadState: PhsaLoadState;
    immunizations: ImmunizationModel[];
}
