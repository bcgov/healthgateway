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
import { WinstonLogger } from "@/services/winstonLogger";

export class RestUserNoteService implements IUserNoteService {
    private logger: ILogger = new WinstonLogger(true); // TODO: inject logger
    private readonly USER_NOTE_BASE_URI: string = "Note";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["GatewayApi"];
        this.http = http;
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.Note);
    }

    public getNotes(hdid: string): Promise<RequestResult<UserNote[]>> {
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
                .getWithCors<RequestResult<UserNote[]>>(
                    `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserNoteService.getNotes()`
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

    public createNote(
        hdid: string,
        note: UserNote
    ): Promise<UserNote | undefined> {
        this.logger.debug(`createNote: ${JSON.stringify(note)}`);
        note.id = undefined;
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve(undefined);
                return;
            }

            this.http
                .post<RequestResult<UserNote>>(
                    `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`,
                    note
                )
                .then((requestResult) =>
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    )
                )
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserNoteService.createNote()`
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

    public updateNote(hdid: string, note: UserNote): Promise<UserNote> {
        return new Promise((resolve, reject) =>
            this.http
                .put<RequestResult<UserNote>>(
                    `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`,
                    note
                )
                .then((requestResult) => {
                    this.logger.debug(`updateNote ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserNoteService.updateNote()`
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

    public deleteNote(hdid: string, note: UserNote): Promise<void> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";

            this.http
                .delete<RequestResult<void>>(
                    `${this.baseUri}${this.USER_NOTE_BASE_URI}/${hdid}`,
                    JSON.stringify(note),
                    headers
                )
                .then((requestResult) => {
                    this.logger.debug(`deleteNote ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserNoteService.deleteNote()`
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
}
