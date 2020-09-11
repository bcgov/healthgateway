// Clinic model
export default interface Clinic {
    // Clinic id.
    clinicId: string;
    // Name.
    name: string;
    // Address line 1.
    addressLine1: string;
    // Address line 2.
    addressLine2: string;
    // Address line 3.
    addressLine3: string;
    // Address line 4.
    addressLine4: string;
    // City.
    city: string;
    // Province.
    province: string;
    // Postal code.
    postalCode: string;
    // Phone number.
    phoneNumber?: string;
}
