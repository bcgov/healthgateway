import { injectable } from "inversify";
import { IHttpDelegate, ICommunicationService } from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import RequestResult from "@/models/requestResult";
import Communication from "@/models/adminCommunication";
import { ResultType } from "@/constants/resulttype";

@injectable()
export class RestCommunicationService implements ICommunicationService {
    private readonly BASE_URI: string = "v1/api/Communication";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public add(communication: Communication): Promise<void> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";
            this.http
                .post<RequestResult<Communication>>(
                    `${this.BASE_URI}`,
                    communication,
                    headers
                )
                .then(requestResult => {
                    if (requestResult.resultStatus == ResultType.Success) {
                        return resolve();
                    } else {
                        return reject(requestResult.resultMessage);
                    }
                })
                .catch(err => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public getAll(): Promise<Communication[]> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<Communication[]>>(`${this.BASE_URI}`)
                .then(requestResult => {
                    this.handleResult(requestResult, resolve, reject);
                })
                .catch(err => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public update(communication: Communication): Promise<void> {
        return new Promise((resolve, reject) => {
            this.http
                .put<RequestResult<void>>(`${this.BASE_URI}/`, communication)
                .then(result => {
                    return this.handleResult(result, resolve, reject);
                })
                .catch(err => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public delete(communication: Communication): Promise<void> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<void>>(`${this.BASE_URI}/`, communication)
                .then(result => {
                    return this.handleResult(result, resolve, reject);
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
