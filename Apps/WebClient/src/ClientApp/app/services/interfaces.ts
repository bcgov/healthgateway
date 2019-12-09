import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
  ExternalConfiguration,
  OpenIdConnectConfiguration
} from "@/models/configData";
import ImmsData from "@/models/immsData";
import PatientData from "@/models/patientData";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import UserFeedback from "@/models/userFeedback";
import { Dictionary } from "vue-router/types/router";
import Pharmacy from "@/models/pharmacy";
import MedicationResult from "@/models/medicationResult";
import MedicationStatement from "@/models/medicationStatement";
import RequestResult from "@/models/requestResult";
import UserEmail from "@/models/emailInvite";

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
  getPatientMedicationStatements(
    hdid: string
  ): Promise<RequestResult<MedicationStatement[]>>;
  getMedicationInformation(drugIdentifier: string): Promise<MedicationResult>;
  getPharmacyInfo(pharmacyId: string): Promise<Pharmacy>;
}

export interface IConfigService {
  initialize(http: IHttpDelegate): void;
  getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
  initialize(http: IHttpDelegate): void;
  createProfile(createRequest: CreateUserRequest): Promise<UserProfile>;
  getProfile(hdid: string): Promise<UserProfile>;
}

export interface IUserFeedbackService {
  initialize(http: IHttpDelegate): void;
  submitFeedback(feedback: UserFeedback): Promise<boolean>;
}

export interface IUserEmailService {
  initialize(http: IHttpDelegate): void;
  getLatestInvite(hdid: string): Promise<UserEmail>;
  validateEmail(inviteKey: string): Promise<boolean>;
}

export interface IHttpDelegate {
  unsetAuthorizationHeader(): void;
  setAuthorizationHeader(accessToken: string): void;
  getWithCors<T>(url: string, headers?: Dictionary<string>): Promise<T>;
  get<T>(url: string, headers?: Dictionary<string>): Promise<T>;
  post<T>(url: string, payload: Object): Promise<T>;
}
