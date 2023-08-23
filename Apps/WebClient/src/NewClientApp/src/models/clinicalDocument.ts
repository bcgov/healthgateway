import { StringISODate } from "./dateWrapper";
import EncodedMedia from "./encodedMedia";
import { ResultError } from "./errors";
import { LoadStatus } from "./storeOperations";

export interface ClinicalDocument {
    id: string;
    fileId: string;
    name: string;
    type: string;
    facilityName: string;
    discipline: string;
    serviceDate: StringISODate;
}

export interface ClinicalDocumentFile {
    fileId: string;
    file?: EncodedMedia;
    error?: ResultError;
    status: LoadStatus;
}
