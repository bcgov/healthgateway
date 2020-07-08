import { injectable } from "inversify";
import { IHttpDelegate, IUserProfileService } from "@/services/interfaces";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { TermsOfService } from "@/models/termsOfService";
import UserEmailInvite from "@/models/userEmailInvite";
import UserSMSInvite from "@/models/userSMSInvite";
import { Dictionary } from "vue-router/types/router";

@injectable()
export class RestUserProfileService implements IUserProfileService {
    private readonly FETCH_ERROR: string = "Fetch error:";
    private readonly USER_PROFILE_BASE_URI: string = "v1/api/UserProfile";
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
                    console.log(this.FETCH_ERROR + err.toString());
                    reject(err);
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
                    console.log(this.FETCH_ERROR + err.toString());
                    reject(err);
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
                    console.log(this.FETCH_ERROR + err.toString());
                    reject(err);
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
                    console.log(this.FETCH_ERROR + err.toString());
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
                    console.log(this.FETCH_ERROR + err.toString());
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
                    console.log(err);
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
                    console.log(err);
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
                    console.log(err);
                    return resolve(err);
                });
        });
    }

    public getLatestSMSInvite(hdid: string): Promise<UserSMSInvite> {
        return new Promise((resolve) => {
            this.http
                .get<UserSMSInvite>(
                    `${this.USER_PROFILE_BASE_URI}/${hdid}/sms/invite/`
                )
                .then((userSMSInvite) => {
                    return resolve(userSMSInvite);
                })
                .catch((err) => {
                    console.log(err);
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
                    console.log(err);
                    return resolve(err);
                });
        });
    }

    public updateSMSNumber(hdid: string, smsNumber: string): Promise<boolean> {
        return new Promise((resolve) => {
            let headers: Dictionary<string> = {};
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
                    console.log(err);
                    return resolve(err);
                });
        });
    }

    private handleResult(
        requestResult: RequestResult<any>,
        resolve: any,
        reject: any
    ) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultMessage);
        }
    }
}
