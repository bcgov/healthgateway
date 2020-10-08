import { injectable } from "inversify";
import { IHttpDelegate, IBetaRequestService } from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import BetaRequest from "@/models/userBetaRequest";
import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";

@injectable()
export class RestBetaRequestService implements IBetaRequestService {
    private readonly BETA_REQUEST_BASE_URI: string = "v1/api/BetaRequest";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getPendingRequests(): Promise<BetaRequest[]> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<BetaRequest[]>>(
                    `${this.BETA_REQUEST_BASE_URI}`
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

    public sendBetaInvites(requestsIds: string[]): Promise<string[]> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";
            this.http
                .patch<RequestResult<string[]>>(
                    `${this.BETA_REQUEST_BASE_URI}`,
                    JSON.stringify(requestsIds),
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

    private handleResult<T>(
        requestResult: RequestResult<T>,
        resolve: (value?: T | PromiseLike<T> | undefined) => void,
        reject: (reason?: unknown) => void
    ) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultMessage);
        }
    }
}
