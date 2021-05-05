import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { RootState } from "@/store/types";

export interface IdleState {
    isVisible: boolean;
}

export interface IdleGetters extends GetterTree<IdleState, RootState> {
    isVisible(state: IdleState): boolean;
}

type StoreContext = ActionContext<IdleState, RootState>;
export interface IdleActions extends ActionTree<IdleState, RootState> {
    setVisibleState(context: StoreContext, isVisible: boolean): void;
}

export interface IdleMutations extends MutationTree<IdleState> {
    setVisibleState(state: IdleState, isVisible: boolean): void;
}

export interface IdleModule extends Module<IdleState, RootState> {
    namespaced: boolean;
    state: IdleState;
    getters: IdleGetters;
    actions: IdleActions;
    mutations: IdleMutations;
}
