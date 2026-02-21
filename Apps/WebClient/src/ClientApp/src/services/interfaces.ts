import AddDependentRequest from "@/models/addDependentRequest";
import { Dictionary } from "@/models/baseTypes";
import { ClinicalDocument } from "@/models/clinicalDocument";
import Communication, { CommunicationType } from "@/models/communication";
import { ExternalConfiguration } from "@/models/configData";
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
} from "@/models/laboratory";
import MedicationRequest from "@/models/medicationRequest";
import MedicationStatement from "@/models/medicationStatement";
import Notification from "@/models/notification";
import Patient from "@/models/patient";
import PatientDataResponse, {
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import Report from "@/models/report";
import ReportRequest from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";
import { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import type { UserComment } from "@/models/userComment";
import UserFeedback from "@/models/userFeedback";
import UserNote from "@/models/userNote";
import type { UserPreference } from "@/models/userPreference";
import UserProfile, {
    CreateUserRequest,
    UserProfileNotificationSettingModel,
} from "@/models/userProfile";
import UserRating from "@/models/userRating";
import VaccinationStatus from "@/models/vaccinationStatus";
import { EventData } from "@/plugins/extensions";

export interface IAuthenticationService {
    signIn(redirectPath: string, idpHint?: string): Promise<OidcTokenDetails>;
    signOut(): Promise<void>;
    refreshToken(): Promise<boolean>;
    getOidcTokenDetails(): OidcTokenDetails | null;
    getOidcUserInfo(): Promise<OidcUserInfo>;
    clearState(): void;
}

export interface IImmunizationService {
    getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationResult>>;
}

export interface IVaccinationStatusService {
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
    getPatient(hdid: string): Promise<Patient>;
}

export interface IMedicationService {
    getPatientMedicationStatements(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatement[]>>;
}

export interface ISpecialAuthorityService {
    getPatientMedicationRequest(
        hdid: string
    ): Promise<RequestResult<MedicationRequest[]>>;
}

export interface IEncounterService {
    getPatientEncounters(hdid: string): Promise<RequestResult<Encounter[]>>;
}

export interface IHospitalVisitService {
    getHospitalVisits(
        hdid: string
    ): Promise<RequestResult<HospitalVisitResult>>;
}

export interface ILaboratoryService {
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
    getRecords(hdid: string): Promise<RequestResult<ClinicalDocument[]>>;
    getFile(fileId: string, hdid: string): Promise<RequestResult<EncodedMedia>>;
}

export interface IConfigService {
    getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
    createProfile(createRequest: CreateUserRequest): Promise<UserProfile>;
    getProfile(hdid: string): Promise<UserProfile>;
    validateAge(hdid: string): Promise<boolean>;
    getTermsOfService(): Promise<TermsOfService>;
    closeAccount(hdid: string): Promise<void>;
    recoverAccount(hdid: string): Promise<void>;
    validateEmail(hdid: string, inviteKey: string): Promise<boolean>;
    validateSms(hdid: string, digit: string): Promise<boolean>;
    updateEmail(hdid: string, email: string): Promise<void>;
    updateSmsNumber(hdid: string, smsNumber: string): Promise<void>;
    updateUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference>;
    createUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference>;
    updateAcceptedTerms(hdid: string, termsOfServiceId: string): Promise<void>;
    isPhoneNumberValid(phoneNumber: string): Promise<boolean>;
    updateNotificationSettings(
        hdid: string,
        notificationSetting: UserProfileNotificationSettingModel
    ): Promise<void>;
}

export interface IUserFeedbackService {
    submitFeedback(hdid: string, feedback: UserFeedback): Promise<boolean>;
}

export interface IUserRatingService {
    submitRating(rating: UserRating): Promise<boolean>;
}

export interface IUserNoteService {
    getNotes(hdid: string): Promise<RequestResult<UserNote[]>>;
    createNote(hdid: string, note: UserNote): Promise<UserNote>;
    updateNote(hdid: string, note: UserNote): Promise<UserNote>;
    deleteNote(hdid: string, note: UserNote): Promise<void>;
}

export interface IUserCommentService {
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
    getNotifications(hdid: string): Promise<Notification[]>;
    dismissNotification(hdid: string, notificationId: string): Promise<void>;
    dismissNotifications(hdid: string): Promise<void>;
}

export interface ICommunicationService {
    getActive(
        type: CommunicationType
    ): Promise<RequestResult<Communication | null>>;
}

export interface IDependentService {
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
    registerTestKit(
        hdid: string,
        testKit: RegisterTestKitRequest
    ): Promise<RegisterTestKitRequest | undefined>;
    registerTestKitPublic(
        testKit: RegisterTestKitRequest
    ): Promise<RegisterTestKitRequest | undefined>;
}

export interface IReportService {
    generateReport(
        reportRequest: ReportRequest
    ): Promise<RequestResult<Report>>;
}

export interface ILogger {
    warn(message: string): void;
    error(message: string): void;
    info(message: string): void;
    verbose(message: string): void;
    debug(message: string): void;
}

export interface IPatientDataService {
    getPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientDataResponse>;
    getFile(hdid: string, fileId: string): Promise<PatientDataFile>;
}

export interface ITrackingService {
    trackEvent(data: EventData): void;
}
