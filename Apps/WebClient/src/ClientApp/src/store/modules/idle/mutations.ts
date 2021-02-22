import { MutationTree } from "vuex";

import { IdleState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<IdleState> = {
    setVisibleState(state: IdleState, isVisible: boolean) {
        logger.verbose(`IdleState:setVisibleState`);
        state.isVisible = isVisible;
    },
};
