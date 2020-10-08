import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
    ExternalConfiguration,
    OpenIdConnectConfiguration,
} from "@/models/configData";
import ImmunizationModel from "@/models/immunizationModel";
import PatientData from "@/models/patientData";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import type { UserComment } from "@/models/userComment";
import UserFeedback from "@/models/userFeedback";
import { Dictionary } from "vue-router/types/router";
import MedicationResult from "@/models/medicationResult";
import RequestResult from "@/models/requestResult";
import UserEmailInvite from "@/models/userEmailInvite";
import BetaRequest from "@/models/betaRequest";
import { TermsOfService } from "@/models/termsOfService";
import UserNote from "@/models/userNote";
import Communication from "@/models/communication";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import UserSMSInvite from "@/models/userSMSInvite";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import UserRating from "@/models/userRating";
import Encounter from "@/models/encounter";
import { OidcUserProfile } from "@/models/user";

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
    getOidcUserProfile(): Promise<OidcUserProfile>;
}

export interface IImmunizationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationModel[]>>;
}

export interface IPatientService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientData(hdid: string): Promise<PatientData>;
}

export interface IMedicationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientMedicationStatementHistory(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatementHistory[]>>;
    getMedicationInformation(drugIdentifier: string): Promise<MedicationResult>;
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
    getTermsOfService(): Promise<TermsOfService>;
    closeAccount(hdid: string): Promise<UserProfile>;
    recoverAccount(hdid: string): Promise<UserProfile>;
    getLatestEmailInvite(hdid: string): Promise<UserEmailInvite>;
    getLatestSMSInvite(hdid: string): Promise<UserSMSInvite | null>;
    validateEmail(hdid: string, inviteKey: string): Promise<boolean>;
    validateSMS(hdid: string, digit: string): Promise<boolean>;
    updateEmail(hdid: string, email: string): Promise<boolean>;
    updateSMSNumber(hdid: string, smsNumber: string): Promise<boolean>;
    updateUserPreference(
        hdid: string,
        preference: string,
        value: string
    ): Promise<boolean>;
}

export interface IUserFeedbackService {
    initialize(http: IHttpDelegate): void;
    submitFeedback(feedback: UserFeedback): Promise<boolean>;
}

export interface IUserRatingService {
    initialize(http: IHttpDelegate): void;
    submitRating(rating: UserRating): Promise<boolean>;
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
    post<T>(url: string, payload: unknown): Promise<T>;
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

export interface ILogger {
    initialize(logLevel?: string): void;
    log(level: string, message: string): void;
    warn(message: string): void;
    error(message: string): void;
    info(message: string): void;
    verbose(message: string): void;
    debug(message: string): void;
}
