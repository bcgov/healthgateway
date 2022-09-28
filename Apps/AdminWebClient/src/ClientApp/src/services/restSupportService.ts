import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import MessageVerification from "@/models/messageVerification";
import RequestResult from "@/models/requestResult";
import { QueryType } from "@/models/userQuery";
import { IHttpDelegate, ISupportService } from "@/services/interfaces";

@injectable()
export class RestSupportService implements ISupportService {
    private readonly BASE_URI: string = "v1/api/Support";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getMessageVerifications(
        type: QueryType,
        query: string
    ): Promise<RequestResult<MessageVerification[]>> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<MessageVerification[]>>(
                    `${this.BASE_URI}/Users?queryType=${type}&queryString=${query}`
                )
                .then((requestResult) => {
                    if (
                        requestResult.resultStatus === ResultType.Success ||
                        requestResult.resultStatus === ResultType.ActionRequired
                    ) {
                        resolve(requestResult);
                    } else {
                        reject(requestResult.resultError);
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
}
