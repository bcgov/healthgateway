import { DataSource } from "@/constants/dataSource";
import { EntryType } from "@/constants/entryType";

export default abstract class DataSourceUtil {
    public static getDataSource(entryType: EntryType): DataSource {
        switch (entryType) {
            case EntryType.ClinicalDocument:
                return DataSource.ClinicalDocument;
            case EntryType.Covid19TestResult:
                return DataSource.Covid19TestResult;
            case EntryType.HealthVisit:
                return DataSource.HealthVisit;
            case EntryType.HospitalVisit:
                return DataSource.HospitalVisit;
            case EntryType.Immunization:
                return DataSource.Immunization;
            case EntryType.LabResult:
                return DataSource.LabResult;
            case EntryType.Medication:
                return DataSource.Medication;
            case EntryType.Note:
                return DataSource.Note;
            case EntryType.SpecialAuthorityRequest:
                return DataSource.SpecialAuthorityRequest;
            case EntryType.DiagnosticImaging:
                return DataSource.DiagnosticImaging;
            case EntryType.BcCancerScreening:
                return DataSource.BcCancerScreening;
            default:
                throw new Error(`Unknown entry type "${entryType}"`);
        }
    }
}
