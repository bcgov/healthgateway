import Address from "./address";

export default class CovidCardMailRequest {
    // Gets or sets the patient health number.
    public personalHealthNumber!: string;

    // Gets or sets the patient covid immunization records.
    public mailAddress!: Address;
}
