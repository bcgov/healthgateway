import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { Dictionary } from "@/models/baseTypes";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";
import type { UserPreference } from "@/models/userPreference";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
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
    private readonly FETCH_ERROR: string = "Fetch error:";
    private readonly USER_PROFILE_BASE_URI: string = "/v1/api/UserProfile";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getProfile(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<RequestResult<UserProfile>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    this.logger.debug(`getProfile ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(`getProfile ${this.FETCH_ERROR}: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
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
                    `${this.USER_PROFILE_BASE_URI}/${createRequest.profile.hdid}`,
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
                .catch((err) => {
                    this.logger.error(
                        `createProfile ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public closeAccount(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<UserProfile>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    this.logger.debug(`closeAccount ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `closeAccount ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public recoverAccount(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<UserProfile>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/recover`
                )
                .then((requestResult) => {
                    this.logger.debug(`recoverAccount ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `recoverAccount ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(err);
                });
        });
    }

    public validateAge(hdid: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<boolean>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/Validate`
                )
                .then((requestResult) => {
                    this.logger.debug(`validateAge ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `validateAge ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(err);
                });
        });
    }

    public getTermsOfService(): Promise<TermsOfService> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<TermsOfService>>(
                    `${this.USER_PROFILE_BASE_URI}/termsofservice`
                )
                .then((requestResult) => {
                    this.logger.debug(`getTermsOfService ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `getTermsOfService ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(err);
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
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/email/validate/${inviteKey}`
                )
                .then((requestResult) => {
                    return resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`validateEmail error: ${err}`);
                    reject(err);
                });
        });
    }

    public validateSMS(hdid: string, digit: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<boolean>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/sms/validate/${digit}`
                )
                .then((requestResult) => {
                    if (requestResult.resultStatus === ResultType.Success) {
                        return resolve(requestResult.resourcePayload);
                    } else {
                        return reject(requestResult.resultError);
                    }
                })
                .catch((err) => {
                    this.logger.error(`validateSMS error: ${err}`);
                    reject(err);
                });
        });
    }

    public updateEmail(hdid: string, email: string): Promise<boolean> {
        return new Promise((resolve) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";

            this.http
                .put<void>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/email`,
                    JSON.stringify(email),
                    headers
                )
                .then(() => {
                    return resolve(true);
                })
                .catch((err) => {
                    this.logger.error(`updateEmail error: ${err}`);
                    return resolve(err);
                });
        });
    }

    public updateSMSNumber(hdid: string, smsNumber: string): Promise<boolean> {
        return new Promise((resolve) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";

            this.http
                .put<void>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/sms`,
                    JSON.stringify(smsNumber),
                    headers
                )
                .then(() => {
                    return resolve(true);
                })
                .catch((err) => {
                    this.logger.error(`updateSMSNumber error: ${err}`);
                    return resolve(err);
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
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/preference`,
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
                .catch((err) => {
                    this.logger.error(`${this.FETCH_ERROR}: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
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
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/preference`,
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
                .catch((err) => {
                    this.logger.error(
                        `createUserPreference ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
