import { Store } from "vuex";

import AddDependentRequest from "@/models/addDependentRequest";
import ApiResult from "@/models/apiResult";
import { Dictionary } from "@/models/baseTypes";
import { CheckInRequest } from "@/models/checkInRequest";
import ClinicalDocument from "@/models/clinicalDocument";
import Communication, { CommunicationType } from "@/models/communication";
import {
    ExternalConfiguration,
    OpenIdConnectConfiguration,
} from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import EncodedMedia from "@/models/encodedMedia";
import { Encounter } from "@/models/encounter";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import type ImmunizationResult from "@/models/immunizationResult";
import {
    Covid19LaboratoryOrderResult,
    LaboratoryOrderResult,
    LaboratoryReport,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import MedicationRequest from "@/models/MedicationRequest";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import Notification from "@/models/notification";
import PatientData from "@/models/patientData";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import Report from "@/models/report";
import ReportRequest from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";
import { Ticket } from "@/models/ticket";
import { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import type { UserComment } from "@/models/userComment";
import UserFeedback from "@/models/userFeedback";
import UserNote from "@/models/userNote";
import type { UserPreference } from "@/models/userPreference";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import UserRating from "@/models/userRating";
import VaccinationStatus from "@/models/vaccinationStatus";
import { RootState } from "@/store/types";

export interface IAuthenticationService {
    initialize(config: OpenIdConnectConfiguration): Promise<void>;
    signIn(redirectPath: string, idpHint?: string): Promise<OidcTokenDetails>;
    signOut(): Promise<void>;
    refreshToken(): Promise<boolean>;
    getOidcTokenDetails(): OidcTokenDetails | null;
    getOidcUserInfo(): Promise<OidcUserInfo>;
    clearState(): void;
}

export interface IImmunizationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationResult>>;
}

export interface IVaccinationStatusService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPublicVaccineStatus(
        phn: string,
        dateOfBirth: StringISODate,
        dateOfVaccine: StringISODate
    ): Promise<RequestResult<VaccinationStatus>>;
    getPublicVaccineStatusPdf(
        phn: string,
        dateOfBirth: StringISODate,
        dateOfVaccine: StringISODate
    ): Promise<RequestResult<CovidVaccineRecord>>;
    getAuthenticatedVaccineStatus(
        hdid: string
    ): Promise<RequestResult<VaccinationStatus>>;
    getAuthenticatedVaccineRecord(
        hdid: string
    ): Promise<RequestResult<CovidVaccineRecord>>;
}

export interface IPatientService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPatientData(hdid: string): Promise<ApiResult<PatientData>>;
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
    getHospitalVisits(
        hdid: string
    ): Promise<RequestResult<HospitalVisitResult>>;
}

export interface ILaboratoryService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getPublicCovid19Tests(
        phn: string,
        dateOfBirth: string,
        collectionDate: string
    ): Promise<RequestResult<PublicCovidTestResponseResult>>;
    getCovid19LaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<Covid19LaboratoryOrderResult>>;
    getLaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>>;
    getReportDocument(
        reportId: string,
        hdid: string,
        isCovid19: boolean
    ): Promise<RequestResult<LaboratoryReport>>;
}

export interface IClinicalDocumentService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getRecords(hdid: string): Promise<RequestResult<ClinicalDocument[]>>;
    getFile(fileId: string, hdid: string): Promise<RequestResult<EncodedMedia>>;
}

export interface IConfigService {
    initialize(http: IHttpDelegate): void;
    getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
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
    updateAcceptedTerms(
        hdid: string,
        termsOfServiceId: string
    ): Promise<UserProfile>;
}

export interface IUserFeedbackService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    submitFeedback(hdid: string, feedback: UserFeedback): Promise<boolean>;
}

export interface IUserRatingService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
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

export interface INotificationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    getNotifications(hdid: string): Promise<Notification[]>;
    dismissNotification(hdid: string, notificationId: string): Promise<void>;
    dismissNotifications(hdid: string): Promise<void>;
}

export interface ICommunicationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
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

export interface IPcrTestService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    registerTestKit(
        hdid: string,
        testKit: RegisterTestKitRequest
    ): Promise<RegisterTestKitRequest | undefined>;
    registerTestKitPublic(
        testKit: RegisterTestKitRequest
    ): Promise<RegisterTestKitRequest | undefined>;
}

export interface IReportService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
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

export interface ITicketService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
    createTicket(room: string): Promise<Ticket | undefined>;
    removeTicket(checkInRequest: CheckInRequest): Promise<void>;
    updateTicket(checkInRequest: CheckInRequest): Promise<Ticket>;
}
