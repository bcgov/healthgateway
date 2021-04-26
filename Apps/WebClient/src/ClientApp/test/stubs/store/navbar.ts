import {
    NavbarActions,
    NavbarGetters,
    NavbarModule,
    NavbarMutations,
    NavbarState,
} from "@/store/modules/navbar/types";

var navbarState: NavbarState = {
    isSidebarOpen: false,
    isHeaderShown: false,
};

var navbarGetters: NavbarGetters = {
    isSidebarOpen(): boolean {
        return false;
    },
    isHeaderShown(): boolean {
        return false;
    },
};

var navbarActions: NavbarActions = {
    toggleSidebar(): void {},
    setSidebarState(): void {},
    toggleHeader(): void {},
    setHeaderState(): void {},
};

var navbarMutations: NavbarMutations = {
    toggleSidebar(): void {},
    setSidebarState(): void {},
    toggleHeader(): void {},
    setHeaderState(): void {},
};

var navbarStub: NavbarModule = {
    namespaced: true,
    state: navbarState,
    getters: navbarGetters,
    actions: navbarActions,
    mutations: navbarMutations,
};

export default navbarStub;
