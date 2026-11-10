import { EntryType } from "@/constants/entryType";
import { Dataset, Format } from "@/constants/tracking";
import { ReportFormatType } from "@/models/report";

/** Port of utility/eventDataUtility.ts (getFormat, getDataset). */
export function getFormat(reportFormatType: ReportFormatType): Format {
    switch (reportFormatType) {
        case ReportFormatType.CSV:
            return Format.Csv;
        case ReportFormatType.PDF:
            return Format.Pdf;
        case ReportFormatType.XLSX:
            return Format.XLSX;
        default:
            throw new Error(
                `Unknown report format type "${reportFormatType as string}"`
            );
    }
}

export function getDataset(entryType: EntryType | undefined): Dataset {
    switch (entryType) {
        case EntryType.BcCancerScreening:
            return Dataset.BcCancerScreening;
        case EntryType.ClinicalDocument:
            return Dataset.ClinicalDocuments;
        case EntryType.Covid19TestResult:
            return Dataset.Covid19Tests;
        case EntryType.DiagnosticImaging:
            return Dataset.ImagingReports;
        case EntryType.HealthVisit:
            return Dataset.HealthVisits;
        case EntryType.HospitalVisit:
            return Dataset.HospitalVisits;
        case EntryType.Immunization:
            return Dataset.Immunizations;
        case EntryType.LabResult:
            return Dataset.LabResults;
        case EntryType.Medication:
            return Dataset.Medications;
        case EntryType.Note:
            return Dataset.Notes;
        case EntryType.SpecialAuthorityRequest:
            return Dataset.SpecialAuthorityRequests;
        default:
            throw new Error(`Unknown entry type "${entryType}"`);
    }
}
