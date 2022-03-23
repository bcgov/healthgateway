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
    phn: string | null;
    orderingProviderIds: string | null;
    orderingProviders: string | null;
    reportingLab: string | null;
    location: string | null;
    ormOrOru: string | null;
    messageDateTime: StringISODateTime;
    messageId: string | null;
    additionalData: string | null;
    reportAvailable: boolean;
    labResults: Covid19LaboratoryTest[];
}

// COVID-19 lab test model
export interface Covid19LaboratoryTest {
    id: string;
    testType: string | null;
    outOfRange: boolean;
    collectedDateTime: StringISODateTime;
    testStatus: string | null;
    resultDescription: string[];
    resultLink: string | null;
    labResultOutcome: string | null;
    receivedDateTime: StringISODateTime;
    resultDateTime: StringISODateTime;
    loinc: string | null;
    loincName: string | null;
}

// result model for retrieving lab orders
export interface LaboratoryOrderResult {
    loaded: boolean;
    retryin: number;
    orders: LaboratoryOrder[];
}

// laboratory order model
export interface LaboratoryOrder {
    labPdfId: string;
    reportingSource: string;
    reportId: string;
    collectionDateTime: StringISODateTime;
    commonName: string;
    orderingProvider: string;
    testStatus: string;
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

export abstract class LaboratoryUtil {
    public static isTestResultReady(testStatus: string | null): boolean {
        if (testStatus == null) {
            return false;
        } else {
            return ["Final", "Corrected", "Amended"].includes(testStatus);
        }
    }
}
