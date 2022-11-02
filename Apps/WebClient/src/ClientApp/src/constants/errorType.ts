export const enum AppErrorType {
    General,
    TooManyRequests,
}

export const enum ErrorType {
    Create,
    Retrieve,
    Update,
    Delete,
    Download,
    Custom,
}

export enum ErrorSourceType {
    Comment = "comment",
    Dependent = "dependent",
    Encounter = "health visit",
    HospitalVisit = "hospital visit",
    Immunization = "immunization",
    Laboratory = "laboratory result",
    Covid19Laboratory = "COVID‑19 test",
    MedicationRequests = "special authority",
    MedicationStatements = "medication",
    ClinicalDocument = "clinical document",
    Note = "note",
    Patient = "patient",
    VaccineCard = "vaccine card",
    VaccineRecord = "vaccine record",
    LaboratoryReport = "laboratory report",
    Covid19LaboratoryReport = "COVID‑19 laboratory report",
    Profile = "profile",
    User = "user",
    TermsOfService = "terms of service",
    TestKit = "test kit",
    QuickLinks = "quick links",
    DependentImmunizationReport = "Dependent Immunization Report",
    ExportRecords = "Export Records",
}
