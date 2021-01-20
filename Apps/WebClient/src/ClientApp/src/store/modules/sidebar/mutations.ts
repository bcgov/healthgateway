import { MutationTree } from "vuex";

import { SidebarState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<SidebarState> = {
    toggle(state: SidebarState) {
        logger.verbose(`SidebarState:toggle`);
        state.isOpen = !state.isOpen;
    },
    setState(state: SidebarState, isOpen: boolean) {
        logger.verbose(`SidebarState:setState`);
        state.isOpen = isOpen;
    },
};
