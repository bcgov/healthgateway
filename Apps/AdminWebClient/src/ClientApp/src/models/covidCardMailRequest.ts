import Address from "./address";

export default interface CovidCardMailRequest {
    // Gets or sets the patient health number.
    personalHealthNumber: string;

    // Gets or sets the patient covid immunization records.
    mailAddress: Address;
}
