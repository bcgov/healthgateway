import { ResultError } from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ICredentialService, ILogger } from "@/services/interfaces";

import { CredentialActions } from "./types";

export const actions: CredentialActions = {
    createConnection(context, params: { hdid: string }): Promise<boolean> {
        const credentialService: ICredentialService =
            container.get<ICredentialService>(
                SERVICE_IDENTIFIER.CredentialService
            );
        return new Promise((resolve, reject) => {
            context.commit("setRequested");
            credentialService
                .createConnection(params.hdid)
                .then((result) => {
                    context.commit("setConnection", result);
                    resolve(true);
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    retrieveConnection(context, params: { hdid: string }): Promise<boolean> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const credentialService: ICredentialService =
            container.get<ICredentialService>(
                SERVICE_IDENTIFIER.CredentialService
            );
        return new Promise((resolve, reject) => {
            if (context.state.connection !== undefined) {
                logger.debug(
                    `Credential Connection found stored, not querying!`
                );
            } else {
                context.commit("setRequested");
                credentialService
                    .getConnection(params.hdid)
                    .then((result) => {
                        context.commit("setConnection", result);
                        resolve(true);
                    })
                    .catch((error) => {
                        context.dispatch("handleError", error);
                        reject(error);
                    });
            }
        });
    },
    createCredential(
        context,
        params: { hdid: string; targetId: string }
    ): Promise<boolean> {
        const credentialService: ICredentialService =
            container.get<ICredentialService>(
                SERVICE_IDENTIFIER.CredentialService
            );
        return new Promise((resolve, reject) => {
            context.commit("setRequested");
            credentialService
                .createCredential(params.hdid, params.targetId)
                .then((result) => {
                    context.commit("addCredential", result);
                    resolve(true);
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    revokeCredential(
        context,
        params: { hdid: string; credentialId: string }
    ): Promise<boolean> {
        const credentialService: ICredentialService =
            container.get<ICredentialService>(
                SERVICE_IDENTIFIER.CredentialService
            );
        return new Promise((resolve, reject) => {
            context.commit("setRequested");
            credentialService
                .revokeCredential(params.hdid, params.credentialId)
                .then((result) => {
                    context.commit("removeCredential", result);
                    resolve(true);
                })
                .catch((error) => {
                    context.dispatch("handleError", error);
                    reject(error);
                });
        });
    },
    handleError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("credentialError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Credentials Error", error },
            { root: true }
        );
    },
};
