import { RootState } from "@/store/types";
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

export interface NavbarState {
    isSidebarOpen: boolean;
    isHeaderShown: boolean;
}

export interface NavbarGetters extends GetterTree<NavbarState, RootState> {
    isSidebarOpen(state: NavbarState): boolean;
    isHeaderShown(state: NavbarState): boolean;
}

type StoreContext = ActionContext<NavbarState, RootState>;
export interface NavbarActions extends ActionTree<NavbarState, RootState> {
    toggleSidebar(context: StoreContext): void;
    setSidebarState(context: StoreContext, isOpen: boolean): void;
    toggleHeader(context: StoreContext): void;
    setHeaderState(context: StoreContext, isOpen: boolean): void;
}

export interface NavbarMutations extends MutationTree<NavbarState> {
    toggleSidebar(state: NavbarState): void;
    setSidebarState(state: NavbarState, isOpen: boolean): void;
    toggleHeader(state: NavbarState): void;
    setHeaderState(state: NavbarState, isOpen: boolean): void;
}

export interface NavbarModule extends Module<NavbarState, RootState> {
    namespaced: boolean;
    state: NavbarState;
    getters: NavbarGetters;
    actions: NavbarActions;
    mutations: NavbarMutations;
}
