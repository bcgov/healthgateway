// Port of constants/errorType.ts
export enum ErrorType {
    Create = "Create",
    Retrieve = "Retrieve",
    Update = "Update",
    Delete = "Delete",
    Download = "Download",
    Custom = "Custom",
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
    Profile = "profile",
    User = "user",
    TermsOfService = "terms of service",
    QuickLinks = "quick links",
    ExportRecords = "export record",
    Notification = "notification",
    OrganDonorRegistration = "organ donor registration",
    DiagnosticImaging = "diagnostic imaging exam",
    PatientData = "patient data",
    BcCancerScreening = "BC Cancer screening",
    DependentImmunizationReport = "dependent immunization report",
    LaboratoryReport = "laboratory report",
    Covid19LaboratoryReport = "COVID‑19 laboratory report",
    MedicationReport = "medication report",
    HealthVisitReport = "health visit report",
    HospitalVisitReport = "hospital visit report",
    ImmunizationReport = "immunization report",
    NoteReport = "note report",
    SpecialAuthorityRequestReport = "special authority report",
}

export enum AppErrorType {
    General = "General",
    TooManyRequests = "TooManyRequests",
}

/** Port of constants/serviceCodes.ts */
export enum ServiceCode {
    DataBase = "DB",
    ClientRegistries = "CR",
    ODR = "ODR",
    HealthGatewayUser = "HGU",
    Medication = "MED",
    SpecialAuthority = "SPA",
    Laboratory = "LAB",
    Immunization = "IMZ",
    Encounter = "ENC",
    HospitalVisit = "HOS",
    Patient = "PAT",
    PHSA = "PHSA",
    Report = "REP",
    ClinicalDocument = "CDO",
    PatientData = "PATD",
}

/** Where a "too many requests" message is shown (TooManyRequestsComponent `location`). */
export const TooManyRequestsLocation = {
    Page: "page",
    VerifySmsModal: "verifySmsModal",
    AddQuickLinkModal: "addQuickLinkModal",
    RecommendationsDialog: "recommendationsDialog",
    NoteDialog: "noteDialog",
    AddDependentDialog: "addDependentDialog",
} as const;
