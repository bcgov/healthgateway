import { GetterTree } from "vuex";

import { LoadStatus, NoteState, RootState } from "@/models/storeState";
import UserNote from "@/models/userNote";

export const getters: GetterTree<NoteState, RootState> = {
    notes(state: NoteState): UserNote[] {
        return state.notes;
    },
    isLoading(state: NoteState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
