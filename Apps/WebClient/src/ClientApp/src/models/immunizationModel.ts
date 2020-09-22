import { StringISODate } from "./dateWrapper";

export default interface ImmunizationModel {
    id: string;
    isSelfReported: boolean;
    location: string;
    name: string;
    dateOfImmunization: StringISODate;
}
