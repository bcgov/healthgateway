import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserNoteService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestUserNoteService implements IUserNoteService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_NOTE_BASE_URI: string = "/v1/api/Note";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Note"];
        this.baseUri = config.serviceEndpoints["GatewayApi"];
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
                .then((requestResult) => {
                    return resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`getNotes error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    NOT_IMPLENTED = "Method not implemented.";

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
                .catch((err) => {
                    this.logger.error(`createNote error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public updateNote(hdid: string, note: UserNote): Promise<UserNote> {
        return new Promise((resolve, reject) => {
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
                .catch((err) => {
                    this.logger.error(`updateNote error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
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
                .catch((err) => {
                    this.logger.error(`deleteNote error: ${err}`);
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
