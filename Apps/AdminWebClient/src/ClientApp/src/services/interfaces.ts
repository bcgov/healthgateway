import { Dictionary } from "vue-router/types/router";

import Communication from "@/models/adminCommunication";
import AuthenticationData from "@/models/authenticationData";
import Email from "@/models/email";
import ExternalConfiguration from "@/models/externalConfiguration";
import UserFeedback, { AdminTag, UserFeedbackTag } from "@/models/userFeedback";

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

export interface IEmailAdminService {
    initialize(http: IHttpDelegate): void;
    getEmails(): Promise<Email[]>;
    resendEmails(emailIds: string[]): Promise<string[]>;
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
}

export interface ICommunicationService {
    initialize(http: IHttpDelegate): void;
    add(communication: Communication): Promise<Communication>;
    update(communication: Communication): Promise<void>;
    getAll(): Promise<Communication[]>;
    delete(communication: Communication): Promise<void>;
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
