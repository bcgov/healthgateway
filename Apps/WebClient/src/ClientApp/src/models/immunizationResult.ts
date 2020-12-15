import ImmunizationModel from "@/models/immunizationModel";
import PhsaLoadState from "@/models/phsaLoadState";

export default interface ImmunizationResult {
    loadState: PhsaLoadState;
    immunizations: ImmunizationModel[];
}
