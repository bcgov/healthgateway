import Vue from "vue";
import { MutationTree } from "vuex";

import { LoadStatus, NoteState } from "@/models/storeState";
import UserNote from "@/models/userNote";

export const mutations: MutationTree<NoteState> = {
    setRequested(state: NoteState) {
        state.status = LoadStatus.REQUESTED;
    },
    setNotes(state: NoteState, notes: UserNote[]) {
        state.notes = notes;
        state.error = undefined;
        state.status = LoadStatus.LOADED;
    },
    addNote(state: NoteState, note: UserNote) {
        state.notes.push(note);
    },
    updateNote(state: NoteState, note: UserNote) {
        const noteIndex = state.notes.findIndex((x) => x.id === note.id);
        Vue.set(state.notes, noteIndex, note);
    },
    deleteNote(state: NoteState, note: UserNote) {
        const noteIndex = state.notes.findIndex((x) => x.id === note.id);
        if (noteIndex > -1) {
            delete state.notes[noteIndex];
            state.notes.splice(noteIndex, 1);
        }
    },
    noteError(state: NoteState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
