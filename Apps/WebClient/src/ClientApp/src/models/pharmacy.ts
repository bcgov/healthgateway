export enum PhoneType {
    Fax = "F",
    Normal = "N",
}

// Pharmacy model
export default interface Pharmacy {
    // Pharmacy id.
    pharmacyId?: string;
    // Name.
    name?: string;
    // Address line 1.
    addressLine1?: string;
    // Address line 2.
    addressLine2?: string;
    // City.
    city?: string;
    // Province.
    province?: string;
    // Postal code.
    postalCode?: string;
    // Country code.
    countryCode?: string;
    // Phone type.
    phoneType?: PhoneType;
    // Phone number.
    phoneNumber?: string;
}
