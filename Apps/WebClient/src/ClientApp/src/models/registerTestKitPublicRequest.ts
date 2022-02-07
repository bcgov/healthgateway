export default interface RegisterTestKitPublicRequest {
    // The patient's PHN.
    phn?: string;

    // The patient's date of birth.
    dob?: string;

    // Patient first name.
    firstName?: string;

    // Patient last name.
    lastName?: string;

    // # of minutes since the test was taken.
    testTakenMinutesAgo?: number;

    // ID of the test kit
    testKitCid?: string;

    // First short code
    shortCodeFirst?: string;

    // Second short code
    shortCodeSecond?: string;

    // The patient's phone number
    contactPhoneNumber?: string;

    // Street address of patient (only required if no phn provided)
    streetAddress?: string;

    // City of patient (only required if no phn provided)
    city?: string;

    // Postal or ZIP code of patient (only required if no phn provided)
    postalOrZip?: string;
}
