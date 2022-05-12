import { StringISODateTime } from "@/models/dateWrapper";

// result model for retrieving COVID-19 lab orders
export interface Covid19LaboratoryOrderResult {
    loaded: boolean;
    retryin: number;
    orders: Covid19LaboratoryOrder[];
}

// COVID-19 lab order model
export interface Covid19LaboratoryOrder {
    id: string;
    phn: string;
    orderingProviderIds: string;
    orderingProviders: string;
    reportingLab: string;
    location: string;
    labType: string;
    messageDateTime: StringISODateTime;
    messageId: string;
    additionalData: string;
    reportAvailable: boolean;
    labResults: Covid19LaboratoryTest[];
}

// COVID-19 lab test model
export interface Covid19LaboratoryTest {
    id: string;
    testType: string;
    outOfRange: boolean;
    collectedDateTime: StringISODateTime;
    testStatus: string;
    resultReady: boolean;
    resultDescription: string[];
    resultLink: string;
    labResultOutcome: string;
    filteredLabResultOutcome: string;
    receivedDateTime: StringISODateTime;
    resultDateTime: StringISODateTime;
    loinc: string;
    loincName: string;
}

// result model for retrieving lab orders
export interface LaboratoryOrderResult {
    loaded: boolean;
    queued: boolean;
    retryin: number;
    orders: LaboratoryOrder[];
}

// laboratory order model
export interface LaboratoryOrder {
    labPdfId: string;
    reportingSource: string;
    reportId: string;
    collectionDateTime: StringISODateTime | null;
    timelineDateTime: StringISODateTime;
    commonName: string;
    orderingProvider: string;
    testStatus: string;
    orderStatus: string;
    downloadLabel: string;
    reportAvailable: boolean;
    laboratoryTests: LaboratoryTest[];
}

// laboratory test model
export interface LaboratoryTest {
    batteryType: string;
    obxId: string;
    outOfRange: boolean;
    loinc: string;
    testStatus: string;
    filteredTestStatus: string;
    result: string;
}

export interface LaboratoryReport {
    mediaType: string;
    encoding: string;
    data: string;
}

export interface PublicCovidTestRecord {
    patientDisplayName: string;
    lab: string;
    reportId: string;
    collectionDateTime: StringISODateTime;
    resultDateTime: StringISODateTime;
    testName: string;
    testType: string;
    testStatus: string;
    testOutcome: string;
    resultTitle: string;
    resultDescription: string[];
    resultLink: string;
}

export interface PublicCovidTestResponseResult {
    loaded: boolean;
    retryin: number;
    records: PublicCovidTestRecord[];
}
