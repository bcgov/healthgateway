import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

import { IdleMutations, IdleState } from "./types";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: IdleMutations = {
    setVisibleState(state: IdleState, isVisible: boolean) {
        logger.verbose(`IdleState:setVisibleState`);
        state.isVisible = isVisible;
    },
};
