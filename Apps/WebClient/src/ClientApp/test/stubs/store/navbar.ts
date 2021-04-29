import {
    NavbarActions,
    NavbarGetters,
    NavbarModule,
    NavbarMutations,
    NavbarState,
} from "@/store/modules/navbar/types";

import { stubbedVoid } from "../../utility/stubUtil";

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
    toggleSidebar: stubbedVoid,
    setSidebarState: stubbedVoid,
    toggleHeader: stubbedVoid,
    setHeaderState: stubbedVoid,
};

const navbarMutations: NavbarMutations = {
    toggleSidebar: stubbedVoid,
    setSidebarState: stubbedVoid,
    toggleHeader: stubbedVoid,
    setHeaderState: stubbedVoid,
};

const navbarStub: NavbarModule = {
    namespaced: true,
    state: navbarState,
    getters: navbarGetters,
    actions: navbarActions,
    mutations: navbarMutations,
};

export default navbarStub;
