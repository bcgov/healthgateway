import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { UserPreference } from "@/models/userPreference";
import type {
    CreateUserRequest,
    TermsOfService,
    UserProfile,
    UserProfileNotificationSettingModel,
} from "@/models/userProfile";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IUserProfileService {
    getProfile(hdid: string): Promise<UserProfile>;
    getProfileForLogin(hdid: string): Promise<UserProfile>;
    closeAccount(hdid: string): Promise<void>;
    recoverAccount(hdid: string): Promise<void>;
    getTermsOfService(): Promise<TermsOfService>;
    validateSms(hdid: string, digits: string): Promise<boolean>;
    updateEmail(hdid: string, email: string): Promise<void>;
    updateSmsNumber(hdid: string, smsNumber: string): Promise<void>;
    isPhoneNumberValid(phoneNumber: string): Promise<boolean>;
    updateNotificationSettings(
        hdid: string,
        setting: UserProfileNotificationSettingModel
    ): Promise<void>;
    validateAge(hdid: string): Promise<boolean>;
    createProfile(request: CreateUserRequest): Promise<UserProfile>;
    updateAcceptedTerms(hdid: string, termsOfServiceId: string): Promise<void>;
    validateEmail(hdid: string, inviteKey: string): Promise<boolean>;
    createUserPreference(
        hdid: string,
        preference: UserPreference
    ): Promise<UserPreference>;
    updateUserPreference(
        hdid: string,
        preference: UserPreference
    ): Promise<UserPreference>;
}

const API_VERSION = "2.0";
const JSON_HEADERS = { "Content-Type": "application/json; charset=utf-8" };

export class UserProfileService implements IUserProfileService {
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = `${config.serviceEndpoints["GatewayApi"]}UserProfile`;
    }

    private async call<T>(name: string, fn: () => Promise<T>): Promise<T> {
        try {
            return await fn();
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(`Error in UserProfileService.${name}()`, error);
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }

    getProfile(hdid: string) {
        return this.call("getProfile", () =>
            this.http.get<UserProfile>(
                `${this.baseUri}/${hdid}?api-version=${API_VERSION}`
            )
        );
    }

    getProfileForLogin(hdid: string) {
        return this.call("getProfileForLogin", () =>
            this.http.get<UserProfile>(
                `${this.baseUri}/${hdid}/login?api-version=${API_VERSION}`
            )
        );
    }

    closeAccount(hdid: string) {
        return this.call("closeAccount", () =>
            this.http.delete<void>(
                `${this.baseUri}/${hdid}?api-version=${API_VERSION}`
            )
        );
    }

    recoverAccount(hdid: string) {
        return this.call("recoverAccount", () =>
            this.http.get<void>(
                `${this.baseUri}/${hdid}/recover?api-version=${API_VERSION}`
            )
        );
    }

    getTermsOfService() {
        return this.call("getTermsOfService", () =>
            this.http.get<TermsOfService>(
                `${this.baseUri}/termsofservice?api-version=${API_VERSION}`
            )
        );
    }

    validateSms(hdid: string, digits: string) {
        return this.call("validateSms", () =>
            this.http.get<boolean>(
                `${this.baseUri}/${hdid}/sms/validate/${digits}?api-version=${API_VERSION}`
            )
        );
    }

    updateEmail(hdid: string, email: string) {
        return this.call("updateEmail", () =>
            this.http.put<void>(
                `${this.baseUri}/${hdid}/email?api-version=${API_VERSION}`,
                JSON.stringify(email),
                JSON_HEADERS
            )
        );
    }

    updateSmsNumber(hdid: string, smsNumber: string) {
        return this.call("updateSmsNumber", () =>
            this.http.put<void>(
                `${this.baseUri}/${hdid}/sms?api-version=${API_VERSION}`,
                JSON.stringify(smsNumber),
                JSON_HEADERS
            )
        );
    }

    isPhoneNumberValid(phoneNumber: string) {
        return this.call("isPhoneNumberValid", () =>
            this.http.get<boolean>(
                `${this.baseUri}/IsValidPhoneNumber/${encodeURIComponent(phoneNumber)}?api-version=${API_VERSION}`
            )
        );
    }

    validateAge(hdid: string) {
        return this.call("validateAge", () =>
            this.http.get<boolean>(
                `${this.baseUri}/${hdid}/Validate?api-version=${API_VERSION}`
            )
        );
    }

    createProfile(request: CreateUserRequest) {
        return this.call("createProfile", () =>
            this.http.post<UserProfile>(
                `${this.baseUri}/${request.profile.hdid}?api-version=${API_VERSION}`,
                request
            )
        );
    }

    updateAcceptedTerms(hdid: string, termsOfServiceId: string) {
        return this.call("updateAcceptedTerms", () =>
            this.http.put<void>(
                `${this.baseUri}/${hdid}/acceptedterms?api-version=${API_VERSION}`,
                JSON.stringify(termsOfServiceId),
                JSON_HEADERS
            )
        );
    }

    validateEmail(hdid: string, inviteKey: string) {
        return this.call("validateEmail", () =>
            this.http.get<boolean>(
                `${this.baseUri}/${hdid}/email/validate/${inviteKey}?api-version=${API_VERSION}`
            )
        );
    }

    createUserPreference(hdid: string, preference: UserPreference) {
        return this.call("createUserPreference", () =>
            this.http.post<UserPreference>(
                `${this.baseUri}/${hdid}/preference?api-version=${API_VERSION}`,
                preference
            )
        );
    }

    updateUserPreference(hdid: string, preference: UserPreference) {
        return this.call("updateUserPreference", () =>
            this.http.put<UserPreference>(
                `${this.baseUri}/${hdid}/preference?api-version=${API_VERSION}`,
                preference
            )
        );
    }

    updateNotificationSettings(
        hdid: string,
        setting: UserProfileNotificationSettingModel
    ) {
        return this.call("updateNotificationSettings", () =>
            this.http.put<void>(
                `${this.baseUri}/${hdid}/notificationsettings?api-version=${API_VERSION}`,
                setting,
                JSON_HEADERS
            )
        );
    }
}
