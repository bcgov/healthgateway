import EncodedMedia from "@/models/encodedMedia";

export default interface CovidVaccineRecord {
    // Gets or sets a value indicating whether the COVID vaccine record has been retrieved.
    loaded: boolean;

    // Gets or sets the minimal amount of milliseconds that should be waited before another request.
    retryin: number;

    // Gets or sets the rendered document.
    document: EncodedMedia;

    // Gets or sets the associated QR code.
    qrCode: EncodedMedia;
}
