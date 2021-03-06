import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import { WalletConnection, WalletCredential } from "@/models/wallet";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    ICredentialService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestCredentialService implements ICredentialService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly CREDENTIAL_BASE_URI: string = "/v1/api/Wallet";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Credential"];
    }

    public createConnection(hdid: string): Promise<WalletConnection> {
        this.logger.debug("createConnection");

        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            this.http
                .post<RequestResult<WalletConnection>>(
                    `${this.CREDENTIAL_BASE_URI}/${hdid}/Connection`,
                    null
                )
                .then((requestResult) => {
                    this.logger.debug(`createConnection ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(`createConnection error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public getConnection(hdid: string): Promise<WalletConnection> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            this.http
                .getWithCors<RequestResult<WalletConnection>>(
                    `${this.CREDENTIAL_BASE_URI}/${hdid}/Connection`
                )
                .then((requestResult) => {
                    this.logger.debug(`getConnection ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(`getConnection error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public disconnectConnection(
        hdid: string,
        connectionId: string
    ): Promise<WalletConnection> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            this.http
                .delete<RequestResult<WalletConnection>>(
                    `${this.CREDENTIAL_BASE_URI}/${hdid}/Connection/${connectionId}`
                )
                .then((requestResult) => {
                    this.logger.debug(`disconnectConnection ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(`disconnectConnection error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public createCredentials(
        hdid: string,
        targetIds: string[]
    ): Promise<WalletCredential> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            this.http
                .post<RequestResult<WalletCredential>>(
                    `${this.CREDENTIAL_BASE_URI}/${hdid}/Credentials`,
                    targetIds
                )
                .then((requestResult) => {
                    this.logger.debug(`createCredential ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(`createCredential error: ${err}`);
                    return reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }

    public revokeCredential(
        hdid: string,
        credentialId: string
    ): Promise<WalletCredential> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            this.http
                .delete<RequestResult<WalletCredential>>(
                    `${this.CREDENTIAL_BASE_URI}/${hdid}/Credential/${credentialId}`
                )
                .then((requestResult) => {
                    this.logger.debug(`revokeCredential ${requestResult}`);
                    RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(`revokeCredential error: ${err}`);
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
