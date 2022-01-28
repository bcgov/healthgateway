import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

import { NavbarMutations, NavbarState } from "./types";

export const mutations: NavbarMutations = {
    toggleSidebar(state: NavbarState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:toggleSidebar`);
        state.isSidebarOpen = !state.isSidebarOpen;
    },
    setSidebarState(state: NavbarState, isOpen: boolean) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setSidebarState`);
        state.isSidebarOpen = isOpen;
    },
    toggleHeader(state: NavbarState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:toggleHeader`);
        state.isHeaderShown = !state.isHeaderShown;
    },
    setHeaderState(state: NavbarState, isOpen: boolean) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setHeaderState`);
        state.isHeaderShown = isOpen;
    },
    setHeaderButtonState(state: NavbarState, visible: boolean) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setHeaderButtonState`);
        state.isHeaderButtonShown = visible;
    },
    setSidebarButtonState(state: NavbarState, visible: boolean) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setSidebarButtonState`);
        state.isSidebarButtonShown = visible;
    },
};
