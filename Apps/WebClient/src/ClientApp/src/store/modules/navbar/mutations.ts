import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { NavbarMutations, NavbarState } from "./types";

export const mutations: NavbarMutations = {
    toggleSidebar(state: NavbarState) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:toggleSidebar`);
        state.isSidebarOpen = !state.isSidebarOpen;
    },
    setSidebarState(state: NavbarState, isOpen: boolean) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setSidebarState`);
        state.isSidebarOpen = isOpen;
    },
    setHeaderState(state: NavbarState, isOpen: boolean) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setHeaderState`);
        state.isHeaderShown = isOpen;
    },
};
