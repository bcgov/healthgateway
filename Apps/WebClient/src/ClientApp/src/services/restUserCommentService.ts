import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
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
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_COMMENT_BASE_URI: string = "/v1/api";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Comment"];
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
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/UserProfile/${hdid}/Comment/Entry?parentEntryId=${parentEntryId}`
                )
                .then((entryComments) => {
                    return resolve(entryComments);
                })
                .catch((err) => {
                    this.logger.error(`getCommentsForEntry error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
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
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/UserProfile/${hdid}/Comment`
                )
                .then((userComments) => {
                    return resolve(userComments);
                })
                .catch((err) => {
                    this.logger.error(`getCommentsForProfile error: ${err}`);
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
    ): Promise<UserComment | undefined> {
        return new Promise<UserComment | undefined>((resolve, reject) => {
            if (!this.isEnabled) {
                resolve(undefined);
                return;
            }
            this.http
                .post<RequestResult<UserComment>>(
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/UserProfile/${hdid}/Comment`,
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
                    this.logger.error(`createComment error: ${err}`);
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
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/UserProfile/${hdid}/Comment`,
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
                    this.logger.error(`updateComment error: ${err}`);
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
                    `${this.baseUri}${this.USER_COMMENT_BASE_URI}/UserProfile/${hdid}/Comment`,
                    comment
                )
                .then((requestResult) =>
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    )
                )
                .catch((err) => {
                    this.logger.error(`deleteComment error: ${err}`);
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
