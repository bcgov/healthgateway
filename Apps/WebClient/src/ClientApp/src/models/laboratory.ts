import { StringISODate } from "./dateWrapper";

// Laboratory model
export interface LaboratoryResult {
    id: string;
    testType: string | null;
    outOfRange: boolean;
    collectedDateTime: StringISODate;
    testStatus: string | null;
    resultDescription: string | null;
    receivedDateTime: StringISODate;
    resultDateTime: StringISODate;
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
    messageDateTime: StringISODate;
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
