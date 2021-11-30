import { StringISODateTime } from "@/models/dateWrapper";

// Laboratory model
export interface LaboratoryResult {
    id: string;
    testType: string | null;
    outOfRange: boolean;
    collectedDateTime: StringISODateTime;
    testStatus: string | null;
    resultDescription: string | null;
    labResultOutcome: string | null;
    receivedDateTime: StringISODateTime;
    resultDateTime: StringISODateTime;
    loinc: string | null;
    loincName: string | null;
}

export interface LaboratoryOrder {
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
    labResults: LaboratoryResult[];
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
    resultDescription: string;
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
