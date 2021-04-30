import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

import { IdleMutations, IdleState } from "./types";

export const mutations: IdleMutations = {
    setVisibleState(state: IdleState, isVisible: boolean) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`IdleState:setVisibleState`);
        state.isVisible = isVisible;
    },
};
