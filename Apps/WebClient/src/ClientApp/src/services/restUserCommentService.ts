import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import type { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserCommentService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestUserCommentService implements IUserCommentService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly USER_COMMENT_BASE_URI: string = "UserProfile";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled =
            config.webClient.featureToggleConfiguration.timeline.comment;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
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
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment/Entry?parentEntryId=${parentEntryId}`
                )
                .then((entryComments) => resolve(entryComments))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserCommentService.getCommentsForEntry()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public getCommentsForProfile(
        hdid: string
    ): Promise<RequestResult<Dictionary<UserComment[]>>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {},
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<Dictionary<UserComment[]>>>(
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`
                )
                .then((userComments) => resolve(userComments))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserCommentService.getCommentsForProfile()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment | undefined> {
        return new Promise<UserComment | undefined>((resolve, reject) => {
            if (!this.isEnabled) {
                resolve(undefined);
                return;
            }
            this.http
                .post<RequestResult<UserComment>>(
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`,
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
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserCommentService.createComment()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment> {
        return new Promise<UserComment>((resolve, reject) =>
            this.http
                .put<RequestResult<UserComment>>(
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`,
                    comment
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `updateComment result: ${JSON.stringify(requestResult)}`
                    );
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserCommentService.updateComment()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }

    public deleteComment(hdid: string, comment: UserComment): Promise<void> {
        return new Promise((resolve, reject) =>
            this.http
                .delete<RequestResult<void>>(
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`,
                    comment
                )
                .then((requestResult) => {
                    this.logger.verbose(
                        `deleteComment result: ${JSON.stringify(requestResult)}`
                    );
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserCommentService.deleteComment()`
                    );
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
}
