<<<<<<< HEAD
import { StringISODate } from "@/models/dateWrapper";

=======
>>>>>>> master
export default interface ImmunizationModel {
    id: string;
    isSelfReported: boolean;
    location: string;
    name: string;
<<<<<<< HEAD
    dateOfImmunization: StringISODate;
=======
    dateOfImmunization: Date;
>>>>>>> master
}
