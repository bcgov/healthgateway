import { EntryType } from "@/constants/entryType";
import { BcCancerScreeningType } from "@/models/patientDataResponse";
import { ReportFormatType } from "@/models/reportRequest";
import { Dataset, Format, Rating, Type } from "@/plugins/extensions";

export default abstract class EventDataUtility {
    public static getFormat(reportFormatType: ReportFormatType): Format {
        switch (reportFormatType) {
            case ReportFormatType.CSV:
                return Format.Csv;
            case ReportFormatType.PDF:
                return Format.Pdf;
            case ReportFormatType.XLSX:
                return Format.XLSX;
            default:
                throw new Error(
                    `Unknown report format type "${reportFormatType}"`
                );
        }
    }

    public static getDataset(entryType: EntryType | undefined): Dataset {
        switch (entryType) {
            case EntryType.BcCancerScreening:
                return Dataset.BcCancer;
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

    public static getRating(value: string): Rating {
        switch (value) {
            case Rating.Skip:
                return Rating.Skip;
            case Rating.One:
                return Rating.One;
            case Rating.Two:
                return Rating.Two;
            case Rating.Three:
                return Rating.Three;
            case Rating.Four:
                return Rating.Four;
            case Rating.Five:
                return Rating.Five;
            default:
                throw new Error(`Unknown rating value "${value}"`);
        }
    }

    public static getType(bcCancerScreeningType: BcCancerScreeningType): Type {
        switch (bcCancerScreeningType) {
            case BcCancerScreeningType.Result:
                return Type.Result;
            case BcCancerScreeningType.Recall:
                return Type.Recall;
            default:
                throw new Error(
                    `Unknown bc cancer screening type "${bcCancerScreeningType}"`
                );
        }
    }
}
