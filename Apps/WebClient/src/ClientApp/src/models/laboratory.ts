/** Port of models/laboratory.ts */
export interface Covid19LaboratoryOrderResult {
    loaded: boolean;
    retryin: number;
    orders: Covid19LaboratoryOrder[];
}

export interface Covid19LaboratoryOrder {
    id: string;
    phn: string;
    orderingProviderIds: string;
    orderingProviders: string;
    reportingLab: string;
    location: string;
    labType: string;
    messageDateTime: string;
    messageId: string;
    additionalData: string;
    reportAvailable: boolean;
    labResults: Covid19LaboratoryTest[];
}

export interface Covid19LaboratoryTest {
    id: string;
    testType: string;
    outOfRange: boolean;
    collectedDateTime: string;
    testStatus: string;
    resultReady: boolean;
    resultDescription: string[];
    resultLink: string;
    labResultOutcome: string;
    filteredLabResultOutcome: string;
    receivedDateTime: string;
    resultDateTime: string;
    loinc: string;
    loincName: string;
}

export interface LaboratoryOrderResult {
    loaded: boolean;
    queued: boolean;
    retryin: number;
    orders: LaboratoryOrder[];
}

export interface LaboratoryOrder {
    labPdfId: string;
    reportingSource: string;
    reportId: string;
    collectionDateTime: string | null;
    timelineDateTime: string;
    commonName: string;
    orderingProvider: string;
    testStatus: string;
    orderStatus: string;
    reportAvailable: boolean;
    laboratoryTests: LaboratoryTest[];
}

export interface LaboratoryTest {
    batteryType: string;
    obxId: string;
    outOfRange: boolean;
    loinc: string;
    testStatus: string;
    result: string;
}

/** Also models/encodedMedia.ts */
export interface LaboratoryReport {
    mediaType: string;
    encoding: string;
    data: string;
}
export type EncodedMedia = LaboratoryReport;
