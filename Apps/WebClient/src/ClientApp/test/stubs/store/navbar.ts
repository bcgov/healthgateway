import {
    NavbarActions,
    NavbarGetters,
    NavbarModule,
    NavbarMutations,
    NavbarState,
} from "@/store/modules/navbar/types";

import { voidMethod } from "../util";

const navbarState: NavbarState = {
    isSidebarOpen: false,
    isHeaderShown: false,
};

const navbarGetters: NavbarGetters = {
    isSidebarOpen(): boolean {
        return false;
    },
    isHeaderShown(): boolean {
        return false;
    },
};

const navbarActions: NavbarActions = {
    toggleSidebar: voidMethod,
    setSidebarState: voidMethod,
    toggleHeader: voidMethod,
    setHeaderState: voidMethod,
};

const navbarMutations: NavbarMutations = {
    toggleSidebar: voidMethod,
    setSidebarState: voidMethod,
    toggleHeader: voidMethod,
    setHeaderState: voidMethod,
};

const navbarStub: NavbarModule = {
    namespaced: true,
    state: navbarState,
    getters: navbarGetters,
    actions: navbarActions,
    mutations: navbarMutations,
};

export default navbarStub;
