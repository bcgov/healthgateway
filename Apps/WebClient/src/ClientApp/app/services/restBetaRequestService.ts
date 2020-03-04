import { injectable } from "inversify";
import { IHttpDelegate, IBetaRequestService } from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import BetaRequest from "@/models/betaRequest";
import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";

@injectable()
export class RestBetaRequestService implements IBetaRequestService {
  private readonly BETA_REQUEST_BASE_URI: string = "v1/api/BetaRequest";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public getRequest(hdid: string): Promise<BetaRequest> {
    return new Promise((resolve, reject) => {
      this.http
        .get<BetaRequest>(`${this.BETA_REQUEST_BASE_URI}/${hdid}`)
        .then(betaRequest => {
          return resolve(betaRequest);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public putRequest(request: BetaRequest): Promise<BetaRequest> {
    return new Promise((resolve, reject) => {
      let headers: Dictionary<string> = {};
      headers["Content-Type"] = "application/json; charset=utf-8";
      this.http
        .put<RequestResult<BetaRequest>>(
          `${this.BETA_REQUEST_BASE_URI}`,
          JSON.stringify(request),
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
    //console.log(requestResult);
    if (requestResult.resultStatus === ResultType.Success) {
      resolve(requestResult.resourcePayload);
    } else {
      reject(requestResult.resultMessage);
    }
  }
}
