import {
    IdleActions,
    IdleGetters,
    IdleModule,
    IdleMutations,
    IdleState,
} from "@/store/modules/idle/types";

import { stubbedVoid } from "../../utility/stubUtil";

const idleState: IdleState = {
    isVisible: false,
};

const idleGetters: IdleGetters = {
    isVisible(): boolean {
        return false;
    },
};

const idleActions: IdleActions = {
    setVisibleState: stubbedVoid,
};

const idleMutations: IdleMutations = {
    setVisibleState: stubbedVoid,
};

const idleStub: IdleModule = {
    namespaced: true,
    state: idleState,
    getters: idleGetters,
    actions: idleActions,
    mutations: idleMutations,
};

export default idleStub;
