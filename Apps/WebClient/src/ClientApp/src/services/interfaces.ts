import {
    SignoutResponse,
    User as OidcUser,
    UserManagerSettings,
} from "oidc-client";
import { Store } from "vuex";

import AddDependentRequest from "@/models/addDependentRequest";
import { Dictionary } from "@/models/baseTypes";
import Communication, { CommunicationType } from "@/models/communication";
import {
    ExternalConfiguration,
    OpenIdConnectConfiguration,
} from "@/models/configData";
import { StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import Encounter from "@/models/encounter";
import type ImmunizationResult from "@/models/immunizationResult";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import MedicationRequest from "@/models/MedicationRequest";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import PatientData from "@/models/patientData";
import Report from "@/models/report";
import ReportRequest from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";
import { OidcUserProfile } from "@/models/user";
import type { UserComment } from "@/models/userComment";
import UserFeedback from "@/models/userFeedback";
import UserNote from "@/models/userNote";
import type { UserPreference } from "@/models/userPreference";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import UserRating from "@/models/userRating";
import VaccinationStatus from "@/models/vaccinationStatus";
import { WalletConnection, WalletCredential } from "@/models/wallet";
import { RootState } from "@/store/types";

export interface IAuthenticationService {
    initialize(config: OpenIdConnectConfiguration, http: IHttpDelegate): void;
    getUser(): Promise<OidcUser | null>;
    logout(): Promise<void>;
    signinSilent(): Promise<OidcUser | null>;
    signinRedirect(idphint: string, redirectPath: string): Promise<void>;
    signinRedirectCallback(): Promise<OidcUser>;
    signoutRedirectCallback(): Promise<SignoutResponse>;
    checkOidcUserSize(user: OidcUser): number;

    getOidcConfig(): UserManagerSettings;
    removeUser(): Promise<void>;
    storeUser(user: OidcUser): Promise<void>;
    clearStaleState(): Promise<void>;
    getOidcUserProfile(): Promise<OidcUserProfile>;
}

export interface IImmunizationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationResult>>;
}

export interface IVaccinationStatusService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getVaccinationStatus(
        phn: string,
        dateOfBirth: StringISODate
    ): Promise<RequestResult<VaccinationStatus>>;
    getReport(
        phn: string,
        dateOfBirth: StringISODate
    ): Promise<RequestResult<Report>>;
}

export interface IPatientService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientData(hdid: string): Promise<RequestResult<PatientData>>;
}

export interface IMedicationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientMedicationStatementHistory(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatementHistory[]>>;
    getPatientMedicationRequest(
        hdid: string
    ): Promise<RequestResult<MedicationRequest[]>>;
}

export interface IEncounterService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientEncounters(hdid: string): Promise<RequestResult<Encounter[]>>;
}

export interface ILaboratoryService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getOrders(hdid: string): Promise<RequestResult<LaboratoryOrder[]>>;
    getReportDocument(
        reportId: string,
        hdid: string
    ): Promise<RequestResult<LaboratoryReport>>;
}

export interface IConfigService {
    initialize(http: IHttpDelegate): void;
    getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
    initialize(http: IHttpDelegate): void;
    createProfile(createRequest: CreateUserRequest): Promise<UserProfile>;
    getProfile(hdid: string): Promise<UserProfile>;
    validateAge(hdid: string): Promise<boolean>;
    getTermsOfService(): Promise<TermsOfService>;
    closeAccount(hdid: string): Promise<UserProfile>;
    recoverAccount(hdid: string): Promise<UserProfile>;
    validateEmail(
        hdid: string,
        inviteKey: string
    ): Promise<RequestResult<boolean>>;
    validateSMS(hdid: string, digit: string): Promise<boolean>;
    updateEmail(hdid: string, email: string): Promise<boolean>;
    updateSMSNumber(hdid: string, smsNumber: string): Promise<boolean>;
    updateUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference>;
    createUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference>;
}

export interface IUserFeedbackService {
    initialize(http: IHttpDelegate): void;
    submitFeedback(hdid: string, feedback: UserFeedback): Promise<boolean>;
}

export interface IUserRatingService {
    initialize(http: IHttpDelegate): void;
    submitRating(rating: UserRating): Promise<boolean>;
}

export interface IUserNoteService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getNotes(hdid: string): Promise<RequestResult<UserNote[]>>;
    createNote(hdid: string, note: UserNote): Promise<UserNote | undefined>;
    updateNote(hdid: string, note: UserNote): Promise<UserNote>;
    deleteNote(hdid: string, note: UserNote): Promise<void>;
}

export interface IUserCommentService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getCommentsForEntry(
        hdid: string,
        parentEntryId: string
    ): Promise<RequestResult<UserComment[]>>;
    getCommentsForProfile(
        hdid: string
    ): Promise<RequestResult<Dictionary<UserComment[]>>>;
    createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment | undefined>;
    updateComment(hdid: string, comment: UserComment): Promise<UserComment>;
    deleteComment(hdid: string, comment: UserComment): Promise<void>;
}

export interface ICommunicationService {
    initialize(http: IHttpDelegate): void;
    getActive(type: CommunicationType): Promise<RequestResult<Communication>>;
}

export interface IDependentService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    addDependent(
        hdid: string,
        dependent: AddDependentRequest
    ): Promise<AddDependentRequest | undefined>;
    getAll(hdid: string): Promise<Dependent[]>;
    removeDependent(hdid: string, dependent: Dependent): Promise<void>;
}

export interface IHttpDelegate {
    unsetAuthorizationHeader(): void;
    setAuthorizationHeader(accessToken: string): void;
    getWithCors<T>(url: string, headers?: Dictionary<string>): Promise<T>;
    get<T>(url: string, headers?: Dictionary<string>): Promise<T>;
    post<T>(
        url: string,
        payload: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
    put<T>(
        url: string,
        payload: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
    patch<T>(
        url: string,
        payload: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
    delete<T>(
        url: string,
        payload?: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
}

export interface ICredentialService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getConnection(hdid: string): Promise<WalletConnection>;
    createConnection(hdid: string): Promise<WalletConnection>;
    disconnectConnection(
        hdid: string,
        connectionId: string
    ): Promise<WalletConnection>;
    createCredentials(
        hdid: string,
        targetIds: string[]
    ): Promise<WalletCredential>;
    revokeCredential(
        hdid: string,
        credentialId: string
    ): Promise<WalletCredential>;
}

export interface IReportService {
    initialize(http: IHttpDelegate): void;
    generateReport(
        reportRequest: ReportRequest
    ): Promise<RequestResult<Report>>;
}

export interface ILogger {
    initialize(logLevel?: string): void;
    log(level: string, message: string): void;
    warn(message: string): void;
    error(message: string): void;
    info(message: string): void;
    verbose(message: string): void;
    debug(message: string): void;
}

export interface IStoreProvider {
    getStore(): Store<RootState>;
}
