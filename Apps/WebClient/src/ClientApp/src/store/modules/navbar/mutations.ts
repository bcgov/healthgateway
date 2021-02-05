import { MutationTree } from "vuex";

import { NavbarState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<NavbarState> = {
    toggleSidebar(state: NavbarState) {
        logger.verbose(`SidebarState:toggleSidebar`);
        state.isSidebarOpen = !state.isSidebarOpen;
    },
    setSidebarState(state: NavbarState, isOpen: boolean) {
        logger.verbose(`SidebarState:setSidebarState`);
        state.isSidebarOpen = isOpen;
    },
    toggleHeader(state: NavbarState) {
        logger.verbose(`SidebarState:toggleHeader`);
        state.isHeaderShown = !state.isHeaderShown;
    },
    setHeaderState(state: NavbarState, isOpen: boolean) {
        logger.verbose(`SidebarState:setHeaderState`);
        state.isHeaderShown = isOpen;
    },
};
