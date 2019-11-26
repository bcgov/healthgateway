import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
  ExternalConfiguration,
  OpenIdConnectConfiguration
} from "@/models/configData";
import ImmsData from "@/models/immsData";
import PatientData from "@/models/patientData";
import RequestResult from "@/models/requestResult";
import UserProfile from "@/models/userProfile";
import UserFeedback from "@/models/userFeedback";
import { Dictionary } from "vue-router/types/router";

export interface IAuthenticationService {
  initialize(config: OpenIdConnectConfiguration, http: IHttpDelegate): void;
  getUser(): Promise<OidcUser | null>;
  logout(): Promise<void>;
  signinSilent(): Promise<OidcUser | null>;
  signinRedirect(idphint: string, redirectPath: string): Promise<void>;
  signinRedirectCallback(): Promise<OidcUser>;
  checkOidcUserSize(user: OidcUser): number;

  getOidcConfig(): UserManagerSettings;
  removeUser(): Promise<void>;
  storeUser(user: OidcUser): Promise<void>;
  clearStaleState(): Promise<void>;
  getOidcUserProfile(): Promise<any>;
}

export interface IImmsService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getItems(): Promise<ImmsData[]>;
}

export interface IPatientService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientData(hdid: string): Promise<PatientData>;
}

export interface IMedicationService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientMedicationStatements(hdid: string): Promise<RequestResult>;
  getMedicationInformation(drugIdentifier: string): Promise<RequestResult>;
  getPharmacyInfo(pharmacyId: string): Promise<RequestResult>;
}

export interface IConfigService {
  initialize(http: IHttpDelegate): void;
  getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
  initialize(http: IHttpDelegate): void;
  createProfile(profile: UserProfile): Promise<boolean>;
  getProfile(hdid: string): Promise<UserProfile>;
}

export interface IUserFeedbackService {
  initialize(http: IHttpDelegate): void;
  submitFeedback(feedback: UserFeedback): Promise<boolean>;
}

export interface IEmailValidationService {
  initialize(http: IHttpDelegate): void;
  validateEmail(inviteKey: string): Promise<boolean>;
}

export interface IHttpDelegate {
  unsetAuthorizationHeader(): void;
  setAuthorizationHeader(accessToken: string): void;
  getWithCors<T>(url: string, headers?: Dictionary<string>): Promise<T>;
  get<T>(url: string, headers?: Dictionary<string>): Promise<T>;
  post<T>(url: string, payload: Object): Promise<T>;
}
