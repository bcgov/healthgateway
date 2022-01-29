import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { RootState } from "@/store/types";

export interface NavbarState {
    isSidebarOpen: boolean;
    isHeaderShown: boolean;
    isSidebarButtonShown: boolean;
    isHeaderButtonShown: boolean;
}

export interface NavbarGetters extends GetterTree<NavbarState, RootState> {
    isHeaderShown(state: NavbarState): boolean;
    isFooterShown(
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean;
    isSidebarOpen(state: NavbarState): boolean;
    isSidebarShown(
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean;
    isSidebarButtonShown(state: NavbarState): boolean;
    isHeaderButtonShown(state: NavbarState): boolean;
}

type StoreContext = ActionContext<NavbarState, RootState>;
export interface NavbarActions extends ActionTree<NavbarState, RootState> {
    toggleSidebar(context: StoreContext): void;
    setSidebarState(context: StoreContext, isOpen: boolean): void;
    toggleHeader(context: StoreContext): void;
    setHeaderState(context: StoreContext, isOpen: boolean): void;
    setSidebarButtonState(context: StoreContext, visible: boolean): void;
    setHeaderButtonState(context: StoreContext, visible: boolean): void;
}

export interface NavbarMutations extends MutationTree<NavbarState> {
    toggleSidebar(state: NavbarState): void;
    setSidebarState(state: NavbarState, isOpen: boolean): void;
    toggleHeader(state: NavbarState): void;
    setHeaderState(state: NavbarState, isOpen: boolean): void;
    setSidebarButtonState(state: NavbarState, visible: boolean): void;
    setHeaderButtonState(state: NavbarState, visible: boolean): void;
}

export interface NavbarModule extends Module<NavbarState, RootState> {
    namespaced: boolean;
    state: NavbarState;
    getters: NavbarGetters;
    actions: NavbarActions;
    mutations: NavbarMutations;
}
