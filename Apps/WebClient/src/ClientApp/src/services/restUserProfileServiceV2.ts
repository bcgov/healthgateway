import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { TermsOfService } from "@/models/termsOfService";
import type { UserPreference } from "@/models/userPreference";
import UserProfile, {
    CreateUserRequest,
    UserProfileNotificationSettingModel,
} from "@/models/userProfile";
import {
    IHttpDelegate,
    ILogger,
    IUserProfileService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestUserProfileServiceV2 implements IUserProfileService {
    private readonly APPLICATION_JSON: string =
        "application/json; charset=utf-8";
    private readonly CONTENT_TYPE: string = "Content-Type";
    private readonly USER_PROFILE_BASE_URI: string = "UserProfile";
    private readonly API_VERSION: string = "2.0";
    private readonly logger;
    private readonly http;
    private readonly baseUri;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public getProfile(hdid: string): Promise<UserProfile> {
        return this.http
            .getWithCors<UserProfile>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.getProfile()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public createProfile(
        createRequest: CreateUserRequest
    ): Promise<UserProfile> {
        return this.http
            .post<UserProfile>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${createRequest.profile.hdid}?api-version=${this.API_VERSION}`,
                createRequest
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.createProfile()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public closeAccount(hdid: string): Promise<void> {
        return this.http
            .delete<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.closeAccount()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public recoverAccount(hdid: string): Promise<void> {
        return this.http
            .get<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/recover?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.recoverAccount()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public validateAge(hdid: string): Promise<boolean> {
        return this.http
            .get<boolean>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/Validate?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.validateAge()`
                );

                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public getTermsOfService(): Promise<TermsOfService> {
        return this.http
            .get<TermsOfService>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/termsofservice?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.getTermsOfService()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public validateEmail(hdid: string, inviteKey: string): Promise<boolean> {
        return this.http
            .get<boolean>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/email/validate/${inviteKey}?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.validateEmail()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public validateSms(hdid: string, digit: string): Promise<boolean> {
        return this.http
            .get<boolean>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/sms/validate/${digit}?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.validateSms()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public updateEmail(hdid: string, email: string): Promise<void> {
        const headers: Dictionary<string> = {};
        headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

        return this.http
            .put<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/email?api-version=${this.API_VERSION}`,
                JSON.stringify(email),
                headers
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.updateEmail()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public updateSmsNumber(hdid: string, smsNumber: string): Promise<void> {
        const headers: Dictionary<string> = {};
        headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

        return this.http
            .put<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/sms?api-version=${this.API_VERSION}`,
                JSON.stringify(smsNumber),
                headers
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.updateSmsNumber()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public updateUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference> {
        return this.http
            .put<UserPreference>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/preference?api-version=${this.API_VERSION}`,
                userPreference
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.updateUserPreference()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public updateAcceptedTerms(
        hdid: string,
        termsOfServiceId: string
    ): Promise<void> {
        const headers: Dictionary<string> = {};
        headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

        return this.http
            .put<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/acceptedterms?api-version=${this.API_VERSION}`,
                JSON.stringify(termsOfServiceId),
                headers
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.updateAcceptedTerms()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public createUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference> {
        return this.http
            .post<UserPreference>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/preference?api-version=${this.API_VERSION}`,
                userPreference
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.createUserPreference()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public isPhoneNumberValid(phoneNumber: string): Promise<boolean> {
        return this.http
            .get<boolean>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/IsValidPhoneNumber/${phoneNumber}?api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.isPhoneNumberValid()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public updateNotificationSettings(
        hdid: string,
        notificationSetting: UserProfileNotificationSettingModel
    ): Promise<void> {
        const headers: Dictionary<string> = {};
        headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

        return this.http
            .put<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/notificationsettings?api-version=${this.API_VERSION}`,
                notificationSetting,
                headers
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.updateNotificationSettings()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }
}
