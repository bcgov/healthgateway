import Vue from "vue";

import { LoadStatus, Operation, OperationType } from "@/models/storeOperations";
import UserNote from "@/models/userNote";

import { NoteMutations, NoteState } from "./types";

export const mutations: NoteMutations = {
    setNotesRequested(state: NoteState) {
        state.status = LoadStatus.REQUESTED;
    },
    setNotes(state: NoteState, notes: UserNote[]) {
        state.notes = notes;
        state.error = undefined;
        state.status = LoadStatus.LOADED;
    },
    addNote(state: NoteState, note: UserNote) {
        state.lastOperation = new Operation(
            note.id as string,
            OperationType.ADD
        );
        state.notes.push(note);
    },
    updateNote(state: NoteState, note: UserNote) {
        const noteIndex = state.notes.findIndex((x) => x.id === note.id);
        state.lastOperation = new Operation(
            note.id as string,
            OperationType.UPDATE
        );
        Vue.set(state.notes, noteIndex, note);
    },
    deleteNote(state: NoteState, note: UserNote) {
        const noteIndex = state.notes.findIndex((x) => x.id === note.id);
        if (noteIndex > -1) {
            state.lastOperation = new Operation(
                note.id as string,
                OperationType.DELETE
            );
            state.notes.splice(noteIndex, 1);
        }
    },
    setNotesError(state: NoteState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
