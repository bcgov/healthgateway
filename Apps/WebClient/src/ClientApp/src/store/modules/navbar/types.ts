import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { RootState } from "@/store/types";

export interface NavbarState {
    isSidebarOpen: boolean | null;
    isHeaderShown: boolean;
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
    isSidebarOpen(
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean;
    isSidebarAvailable(
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean;
}

type StoreContext = ActionContext<NavbarState, RootState>;
export interface NavbarActions extends ActionTree<NavbarState, RootState> {
    toggleSidebar(context: StoreContext): void;
    setSidebarState(context: StoreContext, isOpen: boolean): void;
    setHeaderState(context: StoreContext, isOpen: boolean): void;
}

export interface NavbarMutations extends MutationTree<NavbarState> {
    toggleSidebar(state: NavbarState): void;
    setSidebarState(state: NavbarState, isOpen: boolean): void;
    setHeaderState(state: NavbarState, isOpen: boolean): void;
}

export interface NavbarModule extends Module<NavbarState, RootState> {
    namespaced: boolean;
    state: NavbarState;
    getters: NavbarGetters;
    actions: NavbarActions;
    mutations: NavbarMutations;
}
