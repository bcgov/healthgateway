export const SERVICE_IDENTIFIER = {
  ConfigService: Symbol.for("ConfigService"),
  AuthenticationService: Symbol.for("AuthService"),
  BetaRequestService: Symbol.for("BetaRequestService"),
  DashboardService: Symbol.for("DashboardService"),
  UserFeedbackService: Symbol.for("UserFeedbackService")
};

export const DELEGATE_IDENTIFIER = {
  HttpDelegate: Symbol.for("HttpDelegate")
};
