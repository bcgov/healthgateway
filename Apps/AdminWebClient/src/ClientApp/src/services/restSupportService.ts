import { injectable } from "inversify";

import MessageVerification from "@/models/messageVerification";
import RequestResult from "@/models/requestResult";
import { QueryType } from "@/models/userQuery";
import { IHttpDelegate, ISupportService } from "@/services/interfaces";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestSupportService implements ISupportService {
    private readonly BASE_URI: string = "v1/api/Users";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getMessageVerifications(
        type: QueryType,
        query: string
    ): Promise<MessageVerification[]> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<MessageVerification[]>>(
                    `${this.BASE_URI}?queryType=${type}&queryString=${query}`
                )
                .then((requestResult) => {
                    console.debug(`getMessageVerifications ${requestResult}`);
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
}
