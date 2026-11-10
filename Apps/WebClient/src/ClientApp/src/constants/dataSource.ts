import { EntryType } from "./entryType";

/** Port of constants/dataSource.ts */
export enum DataSource {
    ClinicalDocument = "ClinicalDocument",
    Covid19TestResult = "Covid19TestResult",
    HealthVisit = "HealthVisit",
    HospitalVisit = "HospitalVisit",
    Immunization = "Immunization",
    LabResult = "LabResult",
    Medication = "Medication",
    Note = "Note",
    SpecialAuthorityRequest = "SpecialAuthorityRequest",
    DiagnosticImaging = "DiagnosticImaging",
    OrganDonorRegistration = "OrganDonorRegistration",
    BcCancerScreening = "BcCancerScreening",
}

/** Port of utility/dataSourceUtil.ts */
export function getDataSource(entryType: EntryType): DataSource {
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
            throw new Error(`Unknown entry type "${entryType as string}"`);
    }
}
