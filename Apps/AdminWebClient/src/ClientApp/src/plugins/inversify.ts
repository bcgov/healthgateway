export const SERVICE_IDENTIFIER = {
    ConfigService: Symbol.for("ConfigService"),
    AuthenticationService: Symbol.for("AuthService"),
    DashboardService: Symbol.for("DashboardService"),
    UserFeedbackService: Symbol.for("UserFeedbackService"),
    CommunicationService: Symbol.for("CommunicationService"),
    SupportService: Symbol.for("SupportService"),
    CovidSupportService: Symbol.for("CovidSupportService"),
};

export const DELEGATE_IDENTIFIER = {
    HttpDelegate: Symbol.for("HttpDelegate"),
};
