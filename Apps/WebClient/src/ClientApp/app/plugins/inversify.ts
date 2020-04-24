export const SERVICE_IDENTIFIER = {
  ConfigService: Symbol.for("ConfigService"),
  AuthenticationService: Symbol.for("AuthService"),
  ImmunizationService: Symbol.for("ImmunizationService"),
  PatientService: Symbol.for("PatientService"),
  MedicationService: Symbol.for("MedicationService"),
  UserProfileService: Symbol.for("UserProfileService"),
  UserFeedbackService: Symbol.for("UserFeedbackService"),
  UserEmailService: Symbol.for("UserEmailService"),
  BetaRequestService: Symbol.for("BetaRequestService"),
  UserNoteService: Symbol.for("UserNoteService"),
  CommunicationService: Symbol.for("CommunicationService")
};

export const DELEGATE_IDENTIFIER = {
  HttpDelegate: Symbol.for("HttpDelegate")
};
