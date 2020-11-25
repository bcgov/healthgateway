import { injectable } from "inversify";
import { IHttpDelegate, ICommunicationService } from "@/services/interfaces";
import { Dictionary } from "vue-router/types/router";
import RequestResult from "@/models/requestResult";
import Communication from "@/models/adminCommunication";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestCommunicationService implements ICommunicationService {
    private readonly BASE_URI: string = "v1/api/Communication";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public add(communication: Communication): Promise<Communication> {
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
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
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
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
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
                .then(requestResult => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
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
                .then(requestResult => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch(err => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
}
