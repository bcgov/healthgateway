import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { RequestResult } from "@/models/requestResult";
import type { UserNote } from "@/models/userNote";
import { ConfigUtil } from "@/utils/configUtil";
import { handleResult } from "@/utils/requestResult";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IUserNoteService {
    getNotes(hdid: string): Promise<RequestResult<UserNote[]>>;
    createNote(hdid: string, note: UserNote): Promise<UserNote>;
    updateNote(hdid: string, note: UserNote): Promise<UserNote>;
    deleteNote(hdid: string, note: UserNote): Promise<void>;
}

/** Port of services/restUserNoteService.ts */
export class UserNoteService implements IUserNoteService {
    private readonly USER_NOTE_BASE_URI = "Note";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    private url(hdid: string) {
        return `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`;
    }

    async getNotes(hdid: string): Promise<RequestResult<UserNote[]>> {
        if (!ConfigUtil.isDatasetEnabled(EntryType.Note))
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        try {
            return await this.http.get<RequestResult<UserNote[]>>(
                this.url(hdid)
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestUserNoteService.getNotes()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
    }

    async createNote(hdid: string, note: UserNote): Promise<UserNote> {
        let result: RequestResult<UserNote>;
        try {
            result = await this.http.post<RequestResult<UserNote>>(
                this.url(hdid),
                note
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestUserNoteService.createNote()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
        return handleResult(result);
    }

    async updateNote(hdid: string, note: UserNote): Promise<UserNote> {
        let result: RequestResult<UserNote>;
        try {
            result = await this.http.put<RequestResult<UserNote>>(
                this.url(hdid),
                note
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestUserNoteService.updateNote()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
        return handleResult(result);
    }

    async deleteNote(hdid: string, note: UserNote): Promise<void> {
        let result: RequestResult<void>;
        try {
            result = await this.http.delete<RequestResult<void>>(
                this.url(hdid),
                JSON.stringify(note),
                {
                    "Content-Type": "application/json; charset=utf-8",
                }
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestUserNoteService.deleteNote()");
            throw toResultError(error, ServiceCode.HealthGatewayUser);
        }
        return handleResult(result);
    }
}
