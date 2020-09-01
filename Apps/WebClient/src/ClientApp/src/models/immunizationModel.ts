export default interface ImmunizationModel {
    id: string;
    isSelfReported: boolean;
    location: string;
    type: string;
    immunized: Date;
}
