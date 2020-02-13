import { injectable } from "inversify";
import { IHttpDelegate, IUserFeedbackService } from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import UserFeedback from "@/models/userFeedback";
import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";

@injectable()
export class RestUserFeedbackService implements IUserFeedbackService {
  private readonly USER_FEEDBACK_BASE_URI: string = "v1/api/UserFeedback";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public getFeedbackList(): Promise<UserFeedback[]> {
    return new Promise((resolve, reject) => {
      this.http
        .get<RequestResult<UserFeedback[]>>(`${this.USER_FEEDBACK_BASE_URI}`)
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public toggleReviewed(id: string, version: string, isReviewed: boolean): Promise<boolean> {
    return new Promise((resolve, reject) => {
      let headers: Dictionary<string> = {};
      headers["Content-Type"] = "application/json; charset=utf-8";
      this.http
        .patch<RequestResult<boolean>>(
          `${this.USER_FEEDBACK_BASE_URI}`,
          JSON.stringify({ id, version, isReviewed }),
          headers
        )
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
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
