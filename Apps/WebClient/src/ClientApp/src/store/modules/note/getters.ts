import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";

import { NoteGetters, NoteState } from "./types";

export const getters: NoteGetters = {
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
