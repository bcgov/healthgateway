import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IUserProfileService,
} from "@/services/interfaces";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { TermsOfService } from "@/models/termsOfService";
import UserEmailInvite from "@/models/userEmailInvite";
import UserSMSInvite from "@/models/userSMSInvite";
import { Dictionary } from "vue-router/types/router";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";

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
                    this.handleResult(requestResult, resolve, reject);
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
                    this.handleResult(requestResult, resolve, reject);
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

    public closeAccount(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<UserProfile>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    this.handleResult(requestResult, resolve, reject);
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

    public recoverAccount(hdid: string): Promise<UserProfile> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<UserProfile>>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/recover`
                )
                .then((requestResult) => {
                    this.handleResult(requestResult, resolve, reject);
                })
                .catch((err) => {
                    this.logger.error(`${this.FETCH_ERROR}: ${err}`);
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
                    this.handleResult(requestResult, resolve, reject);
                })
                .catch((err) => {
                    this.logger.error(`${this.FETCH_ERROR}: ${err}`);
                    reject(err);
                });
        });
    }

    public validateEmail(hdid: string, inviteKey: string): Promise<boolean> {
        return new Promise((resolve) => {
            this.http
                .get(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/email/validate/${inviteKey}`
                )
                .then(() => {
                    return resolve(true);
                })
                .catch((err) => {
                    this.logger.error(`validateEmail error: ${err}`);
                    return resolve(false);
                });
        });
    }

    public validateSMS(hdid: string, digit: string): Promise<boolean> {
        return new Promise((resolve) => {
            this.http
                .get(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/sms/validate/${digit}`
                )
                .then(() => {
                    return resolve(true);
                })
                .catch((err) => {
                    this.logger.error(`validateSMS error: ${err}`);
                    return resolve(false);
                });
        });
    }

    public getLatestEmailInvite(hdid: string): Promise<UserEmailInvite> {
        return new Promise((resolve) => {
            this.http
                .get<UserEmailInvite>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/email/invite/`
                )
                .then((userEmailInvite) => {
                    return resolve(userEmailInvite);
                })
                .catch((err) => {
                    this.logger.error(`getLatestEmailInvite error: ${err}`);
                    return resolve(err);
                });
        });
    }

    public getLatestSMSInvite(hdid: string): Promise<UserSMSInvite | null> {
        return new Promise((resolve) => {
            this.http
                .get<UserSMSInvite>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/sms/invite/`
                )
                .then((userSMSInvite) => {
                    return resolve(userSMSInvite);
                })
                .catch((err) => {
                    this.logger.error(`getLatestSMSInvite error: ${err}`);
                    return resolve(err);
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
                    return resolve();
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
                    return resolve();
                })
                .catch((err) => {
                    this.logger.error(`updateSMSNumber error: ${err}`);
                    return resolve(err);
                });
        });
    }

    public updateUserPreference(
        hdid: string,
        preference: string,
        value: string
    ): Promise<boolean> {
        const headers: Dictionary<string> = {};
        headers["Content-Type"] = "application/json; charset=utf-8";
        return new Promise((resolve, reject) => {
            this.http
                .put<boolean>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/preference/${preference}`,
                    JSON.stringify(value),
                    headers
                )
                .then((result) => {
                    resolve(result);
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

    private handleResult<T>(
        requestResult: RequestResult<T>,
        resolve: (value?: T | PromiseLike<T> | undefined) => void,
        reject: (reason?: unknown) => void
    ) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultError);
        }
    }
}
