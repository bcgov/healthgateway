import { voidMethod } from "@test/stubs/util";

import {
    NavbarActions,
    NavbarGetters,
    NavbarModule,
    NavbarMutations,
    NavbarState,
} from "@/store/modules/navbar/types";

const navbarState: NavbarState = {
    isHeaderShown: false,
    isSidebarOpen: false,
    isSidebarButtonShown: false,
    isHeaderButtonShown: false,
};

const navbarGetters: NavbarGetters = {
    isHeaderShown(): boolean {
        return false;
    },
    isFooterShown(): boolean {
        return false;
    },
    isSidebarOpen(): boolean {
        return false;
    },
    isSidebarShown(): boolean {
        return false;
    },
    isHeaderButtonShown(): boolean {
        return false;
    },
    isSidebarButtonShown(): boolean {
        return false;
    },
};

const navbarActions: NavbarActions = {
    toggleSidebar: voidMethod,
    setSidebarState: voidMethod,
    toggleHeader: voidMethod,
    setHeaderState: voidMethod,
    setSidebarButtonState: voidMethod,
    setHeaderButtonState: voidMethod,
};

const navbarMutations: NavbarMutations = {
    toggleSidebar: voidMethod,
    setSidebarState: voidMethod,
    toggleHeader: voidMethod,
    setHeaderState: voidMethod,
    setSidebarButtonState: voidMethod,
    setHeaderButtonState: voidMethod,
};

const navbarStub: NavbarModule = {
    namespaced: true,
    state: navbarState,
    getters: navbarGetters,
    actions: navbarActions,
    mutations: navbarMutations,
};

export default navbarStub;
