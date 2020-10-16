export const SERVICE_IDENTIFIER = {
    ConfigService: Symbol.for("ConfigService"),
    Logger: Symbol.for("Logger"),
    AuthenticationService: Symbol.for("AuthService"),
    ImmunizationService: Symbol.for("ImmunizationService"),
    PatientService: Symbol.for("PatientService"),
    MedicationService: Symbol.for("MedicationService"),
    EncounterService: Symbol.for("EncounterService"),
    LaboratoryService: Symbol.for("LaboratoryService"),
    UserProfileService: Symbol.for("UserProfileService"),
    UserFeedbackService: Symbol.for("UserFeedbackService"),
    UserEmailService: Symbol.for("UserEmailService"),
    BetaRequestService: Symbol.for("BetaRequestService"),
    UserNoteService: Symbol.for("UserNoteService"),
    CommunicationService: Symbol.for("CommunicationService"),
    DependentService: Symbol.for("DependentService"),
    UserCommentService: Symbol.for("UserCommentService"),
    UserRatingService: Symbol.for("UserRatingService"),
};

export const DELEGATE_IDENTIFIER = {
    HttpDelegate: Symbol.for("HttpDelegate"),
};
