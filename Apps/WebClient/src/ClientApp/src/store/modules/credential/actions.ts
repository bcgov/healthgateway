import { ResultError } from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ICredentialService, ILogger } from "@/services/interfaces";

import { CredentialActions } from "./types";

export const actions: CredentialActions = {
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
    retrieveCredentials(context, params: { hdid: string }): Promise<boolean> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const credentialService: ICredentialService =
            container.get<ICredentialService>(
                SERVICE_IDENTIFIER.CredentialService
            );
        return new Promise((resolve, reject) => {
            if (context.state.credentials.length > 0) {
                logger.debug(`Credentials found stored, not querying!`);
            } else {
                context.commit("setRequested");
                credentialService
                    .getCredentials(params.hdid)
                    .then((result) => {
                        context.commit("setCredentials", result);
                        resolve(true);
                    })
                    .catch((error) => {
                        context.dispatch("handleError", error);
                        reject(error);
                    });
            }
        });
    },
    createCredentialConnection(
        context,
        params: { hdid: string; targetIds: string[] }
    ): Promise<boolean> {
        const credentialService: ICredentialService =
            container.get<ICredentialService>(
                SERVICE_IDENTIFIER.CredentialService
            );
        return new Promise((resolve, reject) => {
            context.commit("setRequested");
            credentialService
                .createConnection(params.hdid, params.targetIds)
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
    handleError(context, error: ResultError) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("userError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch Credentials Error", error },
            { root: true }
        );
    },
};
