import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { NavbarMutations, NavbarState } from "./types";

export const mutations: NavbarMutations = {
    setSidebarState(state: NavbarState, isOpen: boolean) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setSidebarState`);
        if (state.isSidebarOpen !== isOpen) {
            state.isSidebarOpen = isOpen;
            state.isSidebarAnimating = true;
        }
    },
    setSidebarStoppedAnimating(state: NavbarState) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setSidebarStoppedAnimating`);
        state.isSidebarAnimating = false;
    },
    setHeaderState(state: NavbarState, isOpen: boolean) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`SidebarState:setHeaderState`);
        state.isHeaderShown = isOpen;
    },
};
