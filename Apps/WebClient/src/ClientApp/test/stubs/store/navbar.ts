import {
    NavbarState,
    NavbarGetters,
    NavbarActions,
    NavbarMutations,
    NavbarModule,
} from "@/store/modules/navbar/types";

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
    toggleSidebar(): void {},
    setSidebarState(): void {},
    toggleHeader(): void {},
    setHeaderState(): void {},
};

const navbarMutations: NavbarMutations = {
    toggleSidebar(state: NavbarState): void {},
    setSidebarState(state: NavbarState, isOpen: boolean): void {},
    toggleHeader(state: NavbarState): void {},
    setHeaderState(state: NavbarState, isOpen: boolean): void {},
};

const navbarStub: NavbarModule = {
    namespaced: true,
    state: navbarState,
    getters: navbarGetters,
    actions: navbarActions,
    mutations: navbarMutations,
};

export default navbarStub;
