import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
  ExternalConfiguration,
  OpenIdConnectConfiguration,
} from "@/models/configData";
import ImmunizationData from "@/models/immunizationData";
import PatientData from "@/models/patientData";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import UserComment from "@/models/userComment";
import UserFeedback from "@/models/userFeedback";
import { Dictionary } from "vue-router/types/router";
import Pharmacy from "@/models/pharmacy";
import MedicationResult from "@/models/medicationResult";
import MedicationStatement from "@/models/medicationStatement";
import RequestResult from "@/models/requestResult";
import UserEmailInvite from "@/models/userEmailInvite";
import BetaRequest from "@/models/betaRequest";
import { TermsOfService } from "@/models/termsOfService";
import UserNote from "@/models/userNote";
import Communication from "@/models/communication";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import UserSMSInvite from "@/models/userSMSInvite";

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

export interface IImmunizationService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientImmunizations(
    hdid: string
  ): Promise<RequestResult<ImmunizationData[]>>;
}

export interface IPatientService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientData(hdid: string): Promise<PatientData>;
}

export interface IMedicationService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientMedicationStatements(
    hdid: string,
    protectiveWord?: string
  ): Promise<RequestResult<MedicationStatement[]>>;
  getPatientMedicationStatementHistory(
    hdid: string,
    protectiveWord?: string
  ): Promise<RequestResult<MedicationStatement[]>>;
  getMedicationInformation(drugIdentifier: string): Promise<MedicationResult>;
  getPharmacyInfo(pharmacyId: string): Promise<Pharmacy>;
}

export interface ILaboratoryService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getOrders(hdid: string): Promise<RequestResult<LaboratoryOrder[]>>;
  getReportDocument(reportId: string): Promise<RequestResult<LaboratoryReport>>;
}

export interface IConfigService {
  initialize(http: IHttpDelegate): void;
  getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
  initialize(http: IHttpDelegate): void;
  createProfile(createRequest: CreateUserRequest): Promise<UserProfile>;
  getProfile(hdid: string): Promise<UserProfile>;
  getTermsOfService(): Promise<TermsOfService>;
  closeAccount(hdid: string): Promise<UserProfile>;
  recoverAccount(hdid: string): Promise<UserProfile>;
  getLatestEmailInvite(hdid: string): Promise<UserEmailInvite>;
  getLatestSMSInvite(hdid: string): Promise<UserSMSInvite>;
  validateEmail(hdid: string, inviteKey: string): Promise<boolean>;
  validateSMS(hdid: string, digit: string): Promise<boolean>;
  updateEmail(hdid: string, email: string): Promise<boolean>;
  updateSMSNumber(hdid: string, smsNumber: string): Promise<boolean>;
}

export interface IUserFeedbackService {
  initialize(http: IHttpDelegate): void;
  submitFeedback(feedback: UserFeedback): Promise<boolean>;
}

export interface IBetaRequestService {
  initialize(http: IHttpDelegate): void;
  getRequest(hdid: string): Promise<BetaRequest>;
  putRequest(request: BetaRequest): Promise<BetaRequest>;
}

export interface IUserNoteService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getNotes(): Promise<RequestResult<UserNote[]>>;
  createNote(note: UserNote): Promise<UserNote>;
  updateNote(note: UserNote): Promise<UserNote>;
  deleteNote(note: UserNote): Promise<void>;
}

export interface IUserCommentService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getCommentsForEntry(
    parentEntryId: string
  ): Promise<RequestResult<UserComment[]>>;
  createComment(comment: UserComment): Promise<UserComment>;
  updateComment(comment: UserComment): Promise<UserComment>;
  deleteComment(comment: UserComment): Promise<void>;
}

export interface ICommunicationService {
  initialize(http: IHttpDelegate): void;
  getActive(): Promise<RequestResult<Communication>>;
}

export interface IHttpDelegate {
  unsetAuthorizationHeader(): void;
  setAuthorizationHeader(accessToken: string): void;
  getWithCors<T>(url: string, headers?: Dictionary<string>): Promise<T>;
  get<T>(url: string, headers?: Dictionary<string>): Promise<T>;
  post<T>(url: string, payload: Object): Promise<T>;
  put<T>(
    url: string,
    payload: Object,
    headers?: Dictionary<string>
  ): Promise<T>;
  patch<T>(
    url: string,
    payload: Object,
    headers?: Dictionary<string>
  ): Promise<T>;
  delete<T>(
    url: string,
    payload?: Object,
    headers?: Dictionary<string>
  ): Promise<T>;
}
