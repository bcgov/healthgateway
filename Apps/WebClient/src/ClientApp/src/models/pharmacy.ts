export enum PhoneType {
    Fax = "F",
    Normal = "N",
}

// Pharmacy model
export default class Pharmacy {
    // Pharmacy id.
    public pharmacyId?: string;
    // Name.
    public name?: string;
    // Address line 1.
    public addressLine1?: string;
    // Address line 2.
    public addressLine2?: string;
    // City.
    public city?: string;
    // Province.
    public province?: string;
    // Postal code.
    public postalCode?: string;
    // Country code.
    public countryCode?: string;
    // Phone type.
    public phoneType?: PhoneType;
    // Phone number.
    public phoneNumber?: string;
}
