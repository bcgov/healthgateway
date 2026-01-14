import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import type { UserComment } from "@/models/userComment";
import {
    IHttpDelegate,
    ILogger,
    IUserCommentService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

export class RestUserCommentService implements IUserCommentService {
    private readonly USER_COMMENT_BASE_URI: string = "UserProfile";
    private readonly logger;
    private readonly http;
    private readonly baseUri;
    private readonly isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
        this.isEnabled =
            config.webClient.featureToggleConfiguration.timeline.comment;
    }

    public getCommentsForEntry(
        hdid: string,
        parentEntryId: string
    ): Promise<RequestResult<UserComment[]>> {
        if (!this.isEnabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .getWithCors<
                RequestResult<UserComment[]>
            >(`${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment/Entry?parentEntryId=${parentEntryId}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserCommentService.getCommentsForEntry()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public getCommentsForProfile(
        hdid: string
    ): Promise<RequestResult<Dictionary<UserComment[]>>> {
        if (!this.isEnabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: {},
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .getWithCors<
                RequestResult<Dictionary<UserComment[]>>
            >(`${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserCommentService.getCommentsForProfile()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment | undefined> {
        if (!this.isEnabled) {
            return Promise.resolve(undefined);
        }

        return this.http
            .post<RequestResult<UserComment>>(
                `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`,
                comment
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserCommentService.createComment()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `createComment result: ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public updateComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment> {
        return this.http
            .put<RequestResult<UserComment>>(
                `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`,
                comment
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserCommentService.updateComment()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `updateComment result: ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public deleteComment(hdid: string, comment: UserComment): Promise<void> {
        return this.http
            .delete<RequestResult<void>>(
                `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`,
                comment
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestUserCommentService.deleteComment()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.verbose(
                    `deleteComment result: ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }
}
