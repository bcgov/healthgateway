import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";

export default interface ImmunizationResult {
    immunizations: ImmunizationEvent[];
    recommendations: Recommendation[];
}
