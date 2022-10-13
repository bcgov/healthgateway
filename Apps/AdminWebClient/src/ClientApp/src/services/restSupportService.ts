import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import MessagingVerification from "@/models/messagingVerification";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import { QueryType } from "@/models/userQuery";
import { IHttpDelegate, ISupportService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestSupportService implements ISupportService {
    private readonly BASE_URI: string = "v1/api/Support";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getUsers(
        type: QueryType,
        query: string
    ): Promise<RequestResult<User[]>> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<User[]>>(
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
                    return reject(ErrorTranslator.internalNetworkError(err));
                });
        });
    }

    public getVerifications(
        hdid: string
    ): Promise<RequestResult<MessagingVerification[]>> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<MessagingVerification[]>>(
                    `${this.BASE_URI}/Verifications?hdid=${hdid}`
                )
                .then((requestResult) => {
                    if (requestResult.resultStatus === ResultType.Success) {
                        resolve(requestResult);
                    } else {
                        reject(requestResult.resultError);
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(ErrorTranslator.internalNetworkError(err));
                });
        });
    }
}
