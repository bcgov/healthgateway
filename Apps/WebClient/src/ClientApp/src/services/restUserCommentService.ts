import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserCommentService,
} from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import type { UserComment } from "@/models/userComment";
import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestUserCommentService implements IUserCommentService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_COMMENT_BASE_URI: string = "/v1/api/Comment";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Comment"];
    }

    public getCommentsForEntry(
        hdid: string,
        parentEntryId: string
    ): Promise<RequestResult<UserComment[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: [],
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<UserComment[]>>(
                    `${this.USER_COMMENT_BASE_URI}/${hdid}?parentEntryId=${parentEntryId}`
                )
                .then((userComments) => {
                    return resolve(userComments);
                })
                .catch((err) => {
                    this.logger.error(err);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment> {
        return new Promise<UserComment>((resolve, reject) => {
            if (!this.isEnabled) {
                reject("comment module is disabled.");
                return;
            }
            this.http
                .post<RequestResult<UserComment>>(
                    `${this.USER_COMMENT_BASE_URI}/${hdid}`,
                    comment
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `createComment result: ${JSON.stringify(requestResult)}`
                    );
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(err);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment> {
        return new Promise<UserComment>((resolve, reject) => {
            this.http
                .put<RequestResult<UserComment>>(
                    `${this.USER_COMMENT_BASE_URI}/${hdid}`,
                    comment
                )
                .then((requestResult) => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(err);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public deleteComment(hdid: string, comment: UserComment): Promise<void> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<void>>(
                    `${this.USER_COMMENT_BASE_URI}/${hdid}`,
                    comment
                )
                .then((requestResult) => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(err);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
