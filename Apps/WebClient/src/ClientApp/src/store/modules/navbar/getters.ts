import { RootState } from "@/store/types";

import { NavbarGetters, NavbarState } from "./types";

export const getters: NavbarGetters = {
    isHeaderShown(state: NavbarState): boolean {
        return state.isHeaderShown;
    },
    isFooterShown: (
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean => {
        const isOffline = rootGetters["config/isOffline"];
        const isAuthenticated: boolean =
            rootGetters["auth/oidcIsAuthenticated"];
        const isValid: boolean = rootGetters["auth/isValidIdentityProvider"];
        const isRegistered: boolean = rootGetters["user/userIsRegistered"];
        return !isOffline && (!isAuthenticated || !isValid || isRegistered);
    },
    isSidebarOpen(
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean {
        // initial sidebar state depends on viewport width
        if (_state.isSidebarOpen === null) {
            const isMobile = rootGetters["isMobile"];
            return isMobile ? false : true;
        }
        return _state.isSidebarOpen;
    },
    isSidebarAnimating(state: NavbarState): boolean {
        return state.isSidebarAnimating;
    },
    isSidebarAvailable: (
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean => {
        const isOffline = rootGetters["config/isOffline"];
        const isAuthenticated: boolean =
            rootGetters["auth/oidcIsAuthenticated"];
        const isValid: boolean = rootGetters["auth/isValidIdentityProvider"];
        const isRegistered: boolean = rootGetters["user/userIsRegistered"];
        const isActive: boolean = rootGetters["user/userIsActive"];
        return (
            !isOffline && isAuthenticated && isValid && isRegistered && isActive
        );
    },
};
