export const SERVICE_IDENTIFIER = {
    ConfigService: Symbol.for("ConfigService"),
    Logger: Symbol.for("Logger"),
    AuthenticationService: Symbol.for("AuthService"),
    ImmunizationService: Symbol.for("ImmunizationService"),
    PatientService: Symbol.for("PatientService"),
    MedicationService: Symbol.for("MedicationService"),
    SpecialAuthorityService: Symbol.for("SpecialAuthorityService"),
    EncounterService: Symbol.for("EncounterService"),
    HospitalVisitService: Symbol.for("HospitalVisitService"),
    LaboratoryService: Symbol.for("LaboratoryService"),
    ClinicalDocumentService: Symbol.for("ClinicalDocumentService"),
    UserProfileService: Symbol.for("UserProfileService"),
    UserFeedbackService: Symbol.for("UserFeedbackService"),
    UserEmailService: Symbol.for("UserEmailService"),
    UserNoteService: Symbol.for("UserNoteService"),
    CommunicationService: Symbol.for("CommunicationService"),
    DependentService: Symbol.for("DependentService"),
    UserCommentService: Symbol.for("UserCommentService"),
    NotificationService: Symbol.for("NotificationService"),
    UserRatingService: Symbol.for("UserRatingService"),
    ReportService: Symbol.for("ReportService"),
    VaccinationStatusService: Symbol.for("VaccinationStatusService"),
    PcrTestService: Symbol.for("PcrTestService"),
    TicketService: Symbol.for("TicketService"),
};

export const DELEGATE_IDENTIFIER = {
    HttpDelegate: Symbol.for("HttpDelegate"),
};

export const STORE_IDENTIFIER = {
    StoreProvider: Symbol.for("StoreWrapper"),
    StoreOptions: Symbol.for("StoreOptions"),
};
