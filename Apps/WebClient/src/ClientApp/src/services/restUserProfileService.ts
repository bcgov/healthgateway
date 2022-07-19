import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";
import type { UserPreference } from "@/models/userPreference";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserProfileService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestUserProfileService implements IUserProfileService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly APPLICATION_JSON: string =
        "application/json; charset=utf-8";
    private readonly CONTENT_TYPE: string = "Content-Type";
    private readonly USER_PROFILE_BASE_URI: string = "UserProfile";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public getProfile(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<UserProfile>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    this.logger.debug(`getProfile ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.getProfile()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public createProfile(
        createRequest: CreateUserRequest
    ): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .post<RequestResult<UserProfile>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${createRequest.profile.hdid}`,
                    createRequest
                )
                .then((requestResult) => {
                    this.logger.debug(`createProfile ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.createProfile()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public closeAccount(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<UserProfile>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    this.logger.debug(`closeAccount ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.closeAccount()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public recoverAccount(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<UserProfile>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/recover`
                )
                .then((requestResult) => {
                    this.logger.debug(`recoverAccount ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.recoverAccount()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public validateAge(hdid: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<boolean>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/Validate`
                )
                .then((requestResult) => {
                    this.logger.debug(`validateAge ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.validateAge()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public getTermsOfService(): Promise<TermsOfService> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<TermsOfService>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/termsofservice`
                )
                .then((requestResult) => {
                    this.logger.debug(`getTermsOfService ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.getTermsOfService()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public validateEmail(
        hdid: string,
        inviteKey: string
    ): Promise<RequestResult<boolean>> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<boolean>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/email/validate/${inviteKey}`
                )
                .then((requestResult) => {
                    return resolve(requestResult);
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.validateEmail()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public validateSMS(hdid: string, digit: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<boolean>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/sms/validate/${digit}`
                )
                .then((requestResult) => {
                    if (requestResult.resultStatus === ResultType.Success) {
                        return resolve(requestResult.resourcePayload);
                    } else {
                        return reject(requestResult.resultError);
                    }
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.validateSMS()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateEmail(hdid: string, email: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

            this.http
                .put<void>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/email`,
                    JSON.stringify(email),
                    headers
                )
                .then(() => {
                    return resolve(true);
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.updateEmail()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateSMSNumber(hdid: string, smsNumber: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;

            this.http
                .put<void>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/sms`,
                    JSON.stringify(smsNumber),
                    headers
                )
                .then(() => {
                    return resolve(true);
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.updateSMSNumber()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference> {
        return new Promise<UserPreference>((resolve, reject) => {
            this.http
                .put<RequestResult<UserPreference>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/preference`,
                    userPreference
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `update user preference result: ${JSON.stringify(
                            requestResult
                        )}`
                    );
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.updateUserPreference()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateAcceptedTerms(
        hdid: string,
        termsOfServiceId: string
    ): Promise<UserProfile> {
        return new Promise<UserProfile>((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.CONTENT_TYPE] = this.APPLICATION_JSON;
            this.http
                .put<RequestResult<UserProfile>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/acceptedterms`,
                    JSON.stringify(termsOfServiceId),
                    headers
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `update user accepted terms result: ${JSON.stringify(
                            requestResult
                        )}`
                    );
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.updateAcceptedTerms()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public createUserPreference(
        hdid: string,
        userPreference: UserPreference
    ): Promise<UserPreference> {
        return new Promise<UserPreference>((resolve, reject) => {
            this.http
                .post<RequestResult<UserPreference>>(
                    `${this.baseUri}${this.USER_PROFILE_BASE_URI}/${hdid}/preference`,
                    userPreference
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `create user preference result: ${JSON.stringify(
                            requestResult
                        )}`
                    );
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserProfileService.createUserPreference()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
