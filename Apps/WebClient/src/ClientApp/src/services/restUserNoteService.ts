import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import {
    IHttpDelegate,
    ILogger,
    IUserNoteService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

export class RestUserNoteService implements IUserNoteService {
    private readonly USER_NOTE_BASE_URI: string = "Note";
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
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.Note);
    }

    public getNotes(hdid: string): Promise<RequestResult<UserNote[]>> {
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
                RequestResult<UserNote[]>
            >(`${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestUserNoteService.getNotes()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            });
    }

    public createNote(hdid: string, note: UserNote): Promise<UserNote> {
        this.logger.debug(`createNote: ${JSON.stringify(note)}`);
        return this.http
            .post<RequestResult<UserNote>>(
                `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`,
                note
            )
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestUserNoteService.createNote()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) =>
                RequestResultUtil.handleResult(requestResult)
            );
    }

    public updateNote(hdid: string, note: UserNote): Promise<UserNote> {
        return this.http
            .put<RequestResult<UserNote>>(
                `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`,
                note
            )
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestUserNoteService.updateNote()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `updateNote ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }

    public deleteNote(hdid: string, note: UserNote): Promise<void> {
        const headers: Dictionary<string> = {};
        headers["Content-Type"] = "application/json; charset=utf-8";

        return this.http
            .delete<RequestResult<void>>(
                `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`,
                JSON.stringify(note),
                headers
            )
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestUserNoteService.deleteNote()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HealthGatewayUser
                );
            })
            .then((requestResult) => {
                this.logger.debug(
                    `deleteNote ${JSON.stringify(requestResult)}`
                );
                return RequestResultUtil.handleResult(requestResult);
            });
    }
}
