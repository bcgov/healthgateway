import { GetterTree } from "vuex";

import {
    LoadStatus,
    NoteState,
    Operation,
    RootState,
} from "@/models/storeState";
import UserNote from "@/models/userNote";

export const getters: GetterTree<NoteState, RootState> = {
    notes(state: NoteState): UserNote[] {
        return state.notes;
    },
    noteCount(state: NoteState): number {
        return state.notes.length;
    },
    lastOperation(state: NoteState): Operation | null {
        return state.lastOperation;
    },
    isLoading(state: NoteState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
