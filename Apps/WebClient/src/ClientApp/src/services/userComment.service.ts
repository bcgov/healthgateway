import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { RequestResult } from "@/models/requestResult";
import type { UserComment } from "@/models/userComment";
import { handleResult } from "@/utils/requestResult";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IUserCommentService {
    getCommentsForEntry(
        hdid: string,
        parentEntryId: string
    ): Promise<RequestResult<UserComment[]>>;
    getCommentsForProfile(
        hdid: string
    ): Promise<RequestResult<Record<string, UserComment[]>>>;
    createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment | undefined>;
    updateComment(hdid: string, comment: UserComment): Promise<UserComment>;
    deleteComment(hdid: string, comment: UserComment): Promise<void>;
}

/** Port of services/restUserCommentService.ts */
export class UserCommentService implements IUserCommentService {
    private readonly USER_COMMENT_BASE_URI = "UserProfile";
    private readonly baseUri: string;
    private readonly isEnabled: boolean;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["GatewayApi"];
        this.isEnabled =
            config.webClient.featureToggleConfiguration.timeline.comment;
    }

    private url(hdid: string) {
        return `${this.baseUri}${this.USER_COMMENT_BASE_URI}/${hdid}/Comment`;
    }

    private async call<T>(name: string, request: () => Promise<T>): Promise<T> {
        try {
            return await request();
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(`Error in RestUserCommentService.${name}()`);
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }

    async getCommentsForEntry(
        hdid: string,
        parentEntryId: string
    ): Promise<RequestResult<UserComment[]>> {
        if (!this.isEnabled)
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        return this.call("getCommentsForEntry", () =>
            this.http.get<RequestResult<UserComment[]>>(
                `${this.url(hdid)}/Entry?parentEntryId=${parentEntryId}`
            )
        );
    }

    async getCommentsForProfile(
        hdid: string
    ): Promise<RequestResult<Record<string, UserComment[]>>> {
        if (!this.isEnabled)
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: {},
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        return this.call("getCommentsForProfile", () =>
            this.http.get<RequestResult<Record<string, UserComment[]>>>(
                this.url(hdid)
            )
        );
    }

    async createComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment | undefined> {
        if (!this.isEnabled) return undefined;
        const result = await this.call("createComment", () =>
            this.http.post<RequestResult<UserComment>>(this.url(hdid), comment)
        );
        return handleResult(result);
    }

    async updateComment(
        hdid: string,
        comment: UserComment
    ): Promise<UserComment> {
        const result = await this.call("updateComment", () =>
            this.http.put<RequestResult<UserComment>>(this.url(hdid), comment)
        );
        return handleResult(result);
    }

    async deleteComment(hdid: string, comment: UserComment): Promise<void> {
        const result = await this.call("deleteComment", () =>
            this.http.delete<RequestResult<void>>(this.url(hdid), comment)
        );
        return handleResult(result);
    }
}
