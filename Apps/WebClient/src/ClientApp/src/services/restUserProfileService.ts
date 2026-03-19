import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError, ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
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
import RequestResultUtil from "@/utility/requestResultUtil";

export class RestUserProfileService implements IUserProfileService {
    private readonly APPLICATION_JSON: string =
        "application/json; charset=utf-8";
    private readonly CONTENT_TYPE: string = "Content-Type";
    private readonly USER_PROFILE_BASE_URI: string = "UserProfile";
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
            .getWithCors<RequestResult<UserProfile>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.getProfile()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `getProfile ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public createProfile(
        createRequest: CreateUserRequest
    ): Promise<UserProfile> {
        return this.http
            .post<RequestResult<UserProfile>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${createRequest.profile.hdid}`,
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
            })
            .then((requestResult) => {
                this.logger.debug(
                    `createProfile ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public closeAccount(hdid: string): Promise<void> {
        return this.http
            .delete<RequestResult<UserProfile>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.closeAccount()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `closeAccount ${JSON.stringify(requestResult)}`
                );
            });
    }

    public recoverAccount(hdid: string): Promise<void> {
        return this.http
            .get<RequestResult<UserProfile>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/recover`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.recoverAccount()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `recoverAccount ${JSON.stringify(requestResult)}`
                );
            });
    }

    public validateAge(hdid: string): Promise<boolean> {
        return this.http
            .get<RequestResult<boolean>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/Validate`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.validateAge()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `validateAge ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public getTermsOfService(): Promise<TermsOfService> {
        return this.http
            .get<RequestResult<TermsOfService>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/termsofservice`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.getTermsOfService()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `getTermsOfService ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public validateEmail(hdid: string, inviteKey: string): Promise<boolean> {
        return this.http
            .get<RequestResult<boolean>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/email/validate/${inviteKey}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.validateEmail()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((result) => {
                if (result.resultError) {
                    const error = ResultError.fromModel(result.resultError);
                    if (result.resourcePayload) {
                        error.statusCode = 409;
                    }

                    throw error;
                }

                return result.resourcePayload;
            });
    }

    public validateSms(hdid: string, digit: string): Promise<boolean> {
        return this.http
            .get<RequestResult<boolean>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/sms/validate/${digit}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.validateSms()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) =>
                RequestResultUtil.handleResult(requestResult)
            );
    }

    public updateEmail(hdid: string, email: string): Promise<void> {
        const headers: Dictionary<string> = {};
        headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

        return this.http
            .put<void>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/email`,
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
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/sms`,
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
            .put<RequestResult<UserPreference>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/preference`,
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
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `update user preference result: ${JSON.stringify(
                        requestResult
                    )}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public updateAcceptedTerms(
        hdid: string,
        termsOfServiceId: string
    ): Promise<void> {
        const headers: Dictionary<string> = {};
        headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

        return this.http
            .put<RequestResult<UserProfile>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/acceptedterms`,
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
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `update user accepted terms result: ${JSON.stringify(
                        requestResult
                    )}`
                );
            });
    }

    public createUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference> {
        return this.http
            .post<RequestResult<UserPreference>>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/preference`,
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
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `create user preference result: ${JSON.stringify(
                        requestResult
                    )}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public isPhoneNumberValid(phoneNumber: string): Promise<boolean> {
        return this.http
            .get<boolean>(
                `${this.baseUri}${this.USER_PROFILE_BASE_URI}/IsValidPhoneNumber/${phoneNumber}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserProfileService.isPhoneNumberValid()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((result: boolean) => {
                this.logger.verbose(
                    `Validate phone number format result: ${result}`
                );
                return result;
            });
    }

    public updateNotificationSettings(
        _hdid: string,
        _notificationSetting: UserProfileNotificationSettingModel
    ): Promise<void> {
        return Promise.reject(new Error("Not implemented."));
    }
}
