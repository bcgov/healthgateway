import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { NoteModule, NoteState } from "./types";

const state: NoteState = {
    notes: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    lastOperation: null,
};

const namespaced = true;

export const note: NoteModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
