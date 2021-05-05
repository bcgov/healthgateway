import { ExternalConfiguration } from "@/models/configData";

import { ConfigActions } from "./types";

export const actions: ConfigActions = {
    initialize(context, config: ExternalConfiguration): void {
        console.log("Initializing the config store...");

        console.log("Configuration: ", config);
        context.commit("configurationLoaded", config);

        console.log("Finished initialization");
    },
};
