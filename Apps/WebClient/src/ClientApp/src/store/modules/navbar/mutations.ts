import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

import { NavbarMutations, NavbarState } from "./types";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: NavbarMutations = {
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
