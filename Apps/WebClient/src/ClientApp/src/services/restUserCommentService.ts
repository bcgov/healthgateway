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
                    `${this.USER_COMMENT_BASE_URI}?parentEntryId=${parentEntryId}`
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

    public createComment(comment: UserComment): Promise<UserComment> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve();
                return;
            }
            this.http
                .post<RequestResult<UserComment>>(
                    `${this.USER_COMMENT_BASE_URI}/`,
                    comment
                )
                .then((result) => {
                    this.logger.verbose(
                        `createComment result: ${JSON.stringify(result)}`
                    );
                    return this.handleResult(result, resolve, reject);
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

    public updateComment(comment: UserComment): Promise<UserComment> {
        return new Promise((resolve, reject) => {
            this.http
                .put<RequestResult<UserComment>>(
                    `${this.USER_COMMENT_BASE_URI}/`,
                    comment
                )
                .then((result) => {
                    return this.handleResult(result, resolve, reject);
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

    public deleteComment(comment: UserComment): Promise<void> {
        return new Promise((resolve, reject) => {
            this.http
                .delete<RequestResult<void>>(
                    `${this.USER_COMMENT_BASE_URI}/`,
                    comment
                )
                .then((result) => {
                    return this.handleResult(result, resolve, reject);
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

    private handleResult<T>(
        requestResult: RequestResult<T>,
        resolve: (value?: T | PromiseLike<T> | undefined) => void,
        reject: (reason?: unknown) => void
    ) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultError);
        }
    }
}
