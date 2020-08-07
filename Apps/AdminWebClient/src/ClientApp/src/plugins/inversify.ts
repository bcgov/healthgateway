export const SERVICE_IDENTIFIER = {
    ConfigService: Symbol.for("ConfigService"),
    AuthenticationService: Symbol.for("AuthService"),
    BetaRequestService: Symbol.for("BetaRequestService"),
    DashboardService: Symbol.for("DashboardService"),
    UserFeedbackService: Symbol.for("UserFeedbackService"),
    EmailAdminService: Symbol.for("EmailAdminService"),
    CommunicationService: Symbol.for("CommunicationService"),
    CsvExportService: Symbol.for("CsvExportService")
};

export const DELEGATE_IDENTIFIER = {
    HttpDelegate: Symbol.for("HttpDelegate")
};
