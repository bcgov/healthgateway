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
    isSidebarAnimating: boolean;
    isHeaderShown: boolean;
}

export interface NavbarGetters extends GetterTree<NavbarState, RootState> {
    isHeaderShown(state: NavbarState): boolean;
    isSidebarOpen(
        _state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        rootState: RootState
    ): boolean;
    isSidebarAnimating(state: NavbarState): boolean;
}

type StoreContext = ActionContext<NavbarState, RootState>;
export interface NavbarActions extends ActionTree<NavbarState, RootState> {
    toggleSidebar(context: StoreContext): void;
    setSidebarStoppedAnimating(context: StoreContext): void;
    setHeaderState(context: StoreContext, isOpen: boolean): void;
}

export interface NavbarMutations extends MutationTree<NavbarState> {
    setSidebarState(state: NavbarState, isOpen: boolean): void;
    setSidebarStoppedAnimating(state: NavbarState): void;
    setHeaderState(state: NavbarState, isOpen: boolean): void;
}

export interface NavbarModule extends Module<NavbarState, RootState> {
    namespaced: boolean;
    state: NavbarState;
    getters: NavbarGetters;
    actions: NavbarActions;
    mutations: NavbarMutations;
}
