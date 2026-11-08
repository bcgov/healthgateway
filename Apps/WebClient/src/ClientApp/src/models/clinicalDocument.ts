import type { LoadStatus } from "./datasetState";
import type { ResultError } from "./errors";
import type { EncodedMedia } from "./laboratory";

/** Port of models/clinicalDocument.ts */
export interface ClinicalDocument {
    id: string;
    fileId: string;
    name: string;
    type: string;
    facilityName: string;
    discipline: string;
    /** ISO date */
    serviceDate: string;
}

export interface ClinicalDocumentFile {
    fileId: string;
    file?: EncodedMedia;
    error?: ResultError;
    status: LoadStatus;
}
