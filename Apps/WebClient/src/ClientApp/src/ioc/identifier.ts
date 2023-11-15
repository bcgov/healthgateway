type DelegateIdentifier = "HttpDelegate";

type ServiceIdentifier =
    | "AuthenticationService"
    | "ClinicalDocumentService"
    | "CommunicationService"
    | "ConfigService"
    | "DelegateService"
    | "DependentService"
    | "EncounterService"
    | "HospitalVisitService"
    | "ImmunizationService"
    | "LaboratoryService"
    | "Logger"
    | "MedicationService"
    | "NotificationService"
    | "PatientDataService"
    | "PatientService"
    | "PcrTestService"
    | "ReportService"
    | "SpecialAuthorityService"
    | "TicketService"
    | "TrackingService"
    | "UserCommentService"
    | "UserEmailService"
    | "UserFeedbackService"
    | "UserNoteService"
    | "UserProfileService"
    | "UserRatingService"
    | "VaccinationStatusService";

export type Identifier = DelegateIdentifier | ServiceIdentifier;

export const DELEGATE_IDENTIFIER: { [key: string]: DelegateIdentifier } = {
    HttpDelegate: "HttpDelegate",
};

export const SERVICE_IDENTIFIER: { [key: string]: ServiceIdentifier } = {
    AuthenticationService: "AuthenticationService",
    ClinicalDocumentService: "ClinicalDocumentService",
    CommunicationService: "CommunicationService",
    ConfigService: "ConfigService",
    DelegateService: "DelegateService",
    DependentService: "DependentService",
    EncounterService: "EncounterService",
    HospitalVisitService: "HospitalVisitService",
    ImmunizationService: "ImmunizationService",
    LaboratoryService: "LaboratoryService",
    Logger: "Logger",
    MedicationService: "MedicationService",
    NotificationService: "NotificationService",
    PatientDataService: "PatientDataService",
    PatientService: "PatientService",
    PcrTestService: "PcrTestService",
    ReportService: "ReportService",
    SpecialAuthorityService: "SpecialAuthorityService",
    TicketService: "TicketService",
    TrackingService: "TrackingService",
    UserCommentService: "UserCommentService",
    UserEmailService: "UserEmailService",
    UserFeedbackService: "UserFeedbackService",
    UserNoteService: "UserNoteService",
    UserProfileService: "UserProfileService",
    UserRatingService: "UserRatingService",
    VaccinationStatusService: "VaccinationStatusService",
};
