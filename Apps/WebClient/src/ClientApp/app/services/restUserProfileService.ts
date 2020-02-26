import { injectable } from "inversify";
import { IHttpDelegate, IUserProfileService } from "@/services/interfaces";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { TermsOfService } from "@/models/termsOfService";

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
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(this.FETCH_ERROR + err.toString());
          reject(err);
        });
    });
  }

  public createProfile(createRequest: CreateUserRequest): Promise<UserProfile> {
    return new Promise((resolve, reject) => {
      this.http
        .post<RequestResult<UserProfile>>(
          `${this.USER_PROFILE_BASE_URI}/${createRequest.profile.hdid}`,
          createRequest
        )
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
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
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
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
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(this.FETCH_ERROR + err.toString());
          reject(err);
        });
    });
  }

  public getTermsOfService(): Promise<TermsOfService> {
    return new Promise((resolve, reject) => {
      this.http
        .get<RequestResult<TermsOfService>>(
          `${this.USER_PROFILE_BASE_URI}/TermsOfService`
        )
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(this.FETCH_ERROR + err.toString());
          reject(err);
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
