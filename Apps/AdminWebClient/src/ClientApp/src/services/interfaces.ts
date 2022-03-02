import { Dictionary } from "vue-router/types/router";

import Communication from "@/models/adminCommunication";
import AuthenticationData from "@/models/authenticationData";
import CovidCardDocumentResult from "@/models/covidCardDocumentResult";
import CovidCardMailRequest from "@/models/covidCardMailRequest";
import CovidCardPatientResult from "@/models/covidCardPatientResult";
import CovidTreatmentAssessmentDetails from "@/models/covidTreatmentAssessmentDetails";
import CovidTreatmentAssessmentRequest from "@/models/covidTreatmentAssessmentRequest";
import ExternalConfiguration from "@/models/externalConfiguration";
import MessageVerification from "@/models/messageVerification";
import UserFeedback, { AdminTag, UserFeedbackTag } from "@/models/userFeedback";
import { QueryType } from "@/models/userQuery";

export interface IConfigService {
    initialize(http: IHttpDelegate): void;
    getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IAuthenticationService {
    initialize(http: IHttpDelegate, config: ExternalConfiguration): void;
    startLoginFlow(redirectUri: string): void;
    getAuthentication(): Promise<AuthenticationData>;
    refreshToken(): Promise<AuthenticationData>;
    destroyToken(): Promise<void>;
}

export interface IUserFeedbackService {
    initialize(http: IHttpDelegate): void;
    getFeedbackList(): Promise<UserFeedback[]>;
    toggleReviewed(feedback: UserFeedback): Promise<boolean>;
    getAllTags(): Promise<AdminTag[]>;
    createTag(feedbackId: string, tagName: string): Promise<UserFeedbackTag>;
    associateTag(feedbackId: string, tag: AdminTag): Promise<UserFeedbackTag>;
    removeTag(feedbackId: string, tag: UserFeedbackTag): Promise<boolean>;
}

export interface IDashboardService {
    initialize(http: IHttpDelegate): void;
    getRegisteredUsersCount(): Promise<{ [key: string]: number }>;
    getLoggedInUsersCount(): Promise<{ [key: string]: number }>;
    getDependentCount(): Promise<{ [key: string]: number }>;
    getRecurrentUserCount(
        days: number,
        startPeriodDate: string,
        endPeriodDate: string
    ): Promise<number>;
    getRatings(
        startPeriodDate: string,
        endPeriodDate: string
    ): Promise<{ [key: string]: number }>;
}

export interface ICommunicationService {
    initialize(http: IHttpDelegate): void;
    add(communication: Communication): Promise<Communication>;
    update(communication: Communication): Promise<void>;
    getAll(): Promise<Communication[]>;
    delete(communication: Communication): Promise<void>;
}

export interface ISupportService {
    initialize(http: IHttpDelegate): void;
    getMessageVerifications(
        type: QueryType,
        query: string
    ): Promise<MessageVerification[]>;
}

export interface ICovidSupportService {
    initialize(http: IHttpDelegate): void;
    getPatient(phn: string, refresh: boolean): Promise<CovidCardPatientResult>;
    retrieveDocument(phn: string): Promise<CovidCardDocumentResult>;
    mailDocument(request: CovidCardMailRequest): Promise<boolean>;
    submitCovidTreatmentAssessment(
        covidTreatmentAssessmentRequest: CovidTreatmentAssessmentRequest
    ): Promise<string>;
    getCovidTreatmentAssessmentDetails(
        phn: string
    ): Promise<CovidTreatmentAssessmentDetails>;
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
