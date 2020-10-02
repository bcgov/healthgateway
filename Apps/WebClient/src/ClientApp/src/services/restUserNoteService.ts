import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IUserNoteService,
} from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import { Dictionary } from "vue-router/types/router";

@injectable()
export class RestUserNoteService implements IUserNoteService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_NOTE_BASE_URI: string = "/v1/api/Note";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Note"];
    }

    public getNotes(): Promise<RequestResult<UserNote[]>> {
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
                    `${this.USER_NOTE_BASE_URI}/`
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

    public createNote(note: UserNote): Promise<UserNote> {
        this.logger.debug(`createNote: ${JSON.stringify(note)}`);
        note.id = undefined;
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve();
                return;
            }

            this.http
                .post<RequestResult<UserNote>>(
                    `${this.USER_NOTE_BASE_URI}/`,
                    note
                )
                .then((result) => {
                    return this.handleResult(result, resolve, reject);
                })
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

    public updateNote(note: UserNote): Promise<UserNote> {
        return new Promise((resolve, reject) => {
            this.http
                .put<RequestResult<UserNote>>(
                    `${this.USER_NOTE_BASE_URI}/`,
                    note
                )
                .then((result) => {
                    return this.handleResult(result, resolve, reject);
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

    public deleteNote(note: UserNote): Promise<void> {
        return new Promise((resolve, reject) => {
            const headers: Dictionary<string> = {};
            headers["Content-Type"] = "application/json; charset=utf-8";

            this.http
                .delete<RequestResult<UserNote>>(
                    `${this.USER_NOTE_BASE_URI}/`,
                    JSON.stringify(note),
                    headers
                )
                .then((result) => {
                    return this.handleResult(result, resolve, reject);
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

    private handleResult(
        requestResult: RequestResult<any>,
        resolve: any,
        reject: any
    ) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultError);
        }
    }
}
