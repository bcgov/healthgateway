import { ActionTree, Commit } from "vuex";

import ExternalConfiguration from "@/models/externalConfiguration";
import { ConfigState, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IConfigService } from "@/services/interfaces";

function handleError(commit: Commit, error: Error) {
    console.log("ERROR:" + error);
    commit("configurationError");
}

export const actions: ActionTree<ConfigState, RootState> = {
    initialize(context): Promise<ExternalConfiguration> {
        console.log("Initializing the config store...");
        const configService: IConfigService = container.get(
            SERVICE_IDENTIFIER.ConfigService
        );

        return new Promise((resolve, reject) => {
            configService
                .getConfiguration()
                .then((value) => {
                    console.log("Configuration: ", value);
                    context.commit("configurationLoaded", value);
                    resolve(value);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                })
                .finally(() => {
                    console.log("Finished initialization");
                });
        });
    },
};
