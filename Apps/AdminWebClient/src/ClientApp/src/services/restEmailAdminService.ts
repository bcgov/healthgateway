import { injectable } from "inversify";
import { IHttpDelegate, IEmailAdminService } from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";
import Email from "@/models/email";

@injectable()
export class RestEmailAdminService implements IEmailAdminService {
  private readonly BASE_URI: string = "v1/api/EmailAdmin";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public getEmails(): Promise<Email[]> {
    return new Promise((resolve, reject) => {
      this.http
        .get<RequestResult<Email[]>>(`${this.BASE_URI}`)
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public resendEmails(emailIds: string[]): Promise<string[]> {
    return new Promise((resolve, reject) => {
      let headers: Dictionary<string> = {};
      headers["Content-Type"] = "application/json; charset=utf-8";
      this.http
        .post<RequestResult<string[]>>(
          `${this.BASE_URI}`,
          JSON.stringify(emailIds),
          headers
        )
        .then(requestResult => {
          resolve();
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
