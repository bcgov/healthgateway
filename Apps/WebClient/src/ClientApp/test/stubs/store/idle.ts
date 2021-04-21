import {
    IdleActions,
    IdleGetters,
    IdleModule,
    IdleMutations,
    IdleState,
} from "@/store/modules/idle/types";

var idleState: IdleState = {
    isVisible: false,
};

var idleGetters: IdleGetters = {
    isVisible(): boolean {
        return false;
    },
};

var idleActions: IdleActions = {
    setVisibleState(): void {},
};

var idleMutations: IdleMutations = {
    setVisibleState(): void {},
};

var idleStub: IdleModule = {
    namespaced: true,
    state: idleState,
    getters: idleGetters,
    actions: idleActions,
    mutations: idleMutations,
};

export default idleStub;
