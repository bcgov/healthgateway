import { injectable } from "inversify";
import { Dictionary } from "vue-router/types/router";

import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";
import UserFeedback, { AdminTag } from "@/models/userFeedback";
import { IHttpDelegate, IUserFeedbackService } from "@/services/interfaces";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestUserFeedbackService implements IUserFeedbackService {
    private readonly BASE_URI: string = "v1/api";
    private readonly USER_FEEDBACK_BASE_URI: string = `${this.BASE_URI}/UserFeedback`;
    private http!: IHttpDelegate;

    private readonly contentTypeHeader = "Content-Type";
    private readonly contentType = "application/json; charset=utf-8";

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getFeedbackList(): Promise<UserFeedback[]> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<UserFeedback[]>>(
                    `${this.USER_FEEDBACK_BASE_URI}`
                )
                .then((requestResult) => {
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

    public toggleReviewed(feedback: UserFeedback): Promise<boolean> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.contentTypeHeader] = this.contentType;
            this.http
                .patch<boolean>(
                    `${this.USER_FEEDBACK_BASE_URI}`,
                    JSON.stringify(feedback),
                    headers
                )
                .then((requestResult) => {
                    if (requestResult) {
                        resolve(true);
                    } else {
                        reject("Error toggling user feedback");
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public getAllTags(): Promise<AdminTag[]> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.contentTypeHeader] = this.contentType;
            this.http
                .get<RequestResult<AdminTag[]>>(`${this.BASE_URI}/Tag`, headers)
                .then((requestResult) => {
                    if (requestResult.resultStatus === ResultType.Success) {
                        resolve(requestResult.resourcePayload);
                    } else {
                        reject("Error retrieving tags");
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public createTag(feedbackId: string, tagName: string): Promise<AdminTag> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.contentTypeHeader] = this.contentType;
            this.http
                .post<RequestResult<AdminTag>>(
                    `${this.USER_FEEDBACK_BASE_URI}/${feedbackId}/Tag`,
                    JSON.stringify(tagName),
                    headers
                )
                .then((requestResult) => {
                    if (requestResult.resultStatus === ResultType.Success) {
                        resolve(requestResult.resourcePayload);
                    } else {
                        reject("Error creating feedback tag");
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public associateTag(feedbackId: string, tag: AdminTag): Promise<AdminTag> {
        console.log(tag);
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.contentTypeHeader] = this.contentType;
            this.http
                .put<RequestResult<AdminTag>>(
                    `${this.USER_FEEDBACK_BASE_URI}/${feedbackId}/Tag`,
                    tag
                )
                .then((requestResult) => {
                    if (requestResult.resultStatus === ResultType.Success) {
                        resolve(requestResult.resourcePayload);
                    } else {
                        reject("Error adding feedback tag");
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public removeTag(feedbackId: string, tag: AdminTag): Promise<boolean> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers[this.contentTypeHeader] = this.contentType;
            this.http
                .delete<boolean>(
                    `${this.USER_FEEDBACK_BASE_URI}/${feedbackId}/Tag`,
                    JSON.stringify(tag),
                    headers
                )
                .then((requestResult) => {
                    if (requestResult) {
                        resolve(true);
                    } else {
                        reject("Error removing feedback tag");
                    }
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
}
