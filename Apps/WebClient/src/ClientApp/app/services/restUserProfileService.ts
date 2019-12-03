import { injectable } from "inversify";
import { IHttpDelegate, IUserProfileService } from "@/services/interfaces";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";

@injectable()
export class RestUserProfileService implements IUserProfileService {
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
          console.log("Fetch error:" + err.toString());
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
          console.log("Fetch error:" + err.toString());
          reject(err);
        });
    });
  }

  public retrieveEmailInvite()
  {}

  private handleResult(
    requestResult: RequestResult<any>,
    resolve: any,
    reject: any
  ) {
    //console.log(requestResult);
    if (requestResult.resultStatus === ResultType.Success) {
      resolve(requestResult.resourcePayload);
    } else {
      reject(requestResult.resultMessage);
    }
  }
}
