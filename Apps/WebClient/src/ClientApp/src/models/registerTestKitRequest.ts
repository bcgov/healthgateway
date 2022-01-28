export default interface RegisterTestKitRequest {
    // The patient's HDID
    hdid?: string;

    // # of minutes since the test was taken.
    testTakenMinutesAgo?: number;

    // ID of the test kit
    testKitCid?: string;
}
