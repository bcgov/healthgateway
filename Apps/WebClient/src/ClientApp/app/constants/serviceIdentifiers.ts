const SERVICE_IDENTIFIER = {
  ConfigService: Symbol.for("ConfigService"),
  AuthenticationService: Symbol.for("AuthService"),
  ImmsService: Symbol.for("ImmsService"),
  PatientService: Symbol.for("PatientService"),
  MedicationService: Symbol.for("MedicationService")
};

export const DELEGATE_IDENTIFIER = {
  HttpDelegate: Symbol.for("HttpDelegate")
};

export default SERVICE_IDENTIFIER;
