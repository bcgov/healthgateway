import { StringISODate, StringISODateTime } from "@/models/dateWrapper";

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

export interface RapidTestRecord {
    testResult: string;
    serialNumber: string;
    testTakenDate: StringISODate;
}

export interface AuthenticatedRapidTestRequest {
    // rapid test serial number.
    labSerialNumber: string;

    // customer personal health number.
    phn?: string;

    // result of the rapid test.
    positive?: boolean;

    // date rapid test was taken.
    dateTestTaken: StringISODate;
}

export interface AuthenticatedRapidTestResponse {
    phn: string;
    records: RapidTestRecord[];
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
