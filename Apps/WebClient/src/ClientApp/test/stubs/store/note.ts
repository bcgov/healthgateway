import { voidPromise, voidMethod } from "@test/stubs/util";

import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import {
    NoteActions,
    NoteGetters,
    NoteModule,
    NoteMutations,
    NoteState,
} from "@/store/modules/note/types";

const noteState: NoteState = {
    notes: [],
    statusMessage: "",
    status: LoadStatus.LOADED,
    lastOperation: null,
};

const noteGetters: NoteGetters = {
    notes(): UserNote[] {
        return [];
    },
    noteCount(): number {
        return 0;
    },
    lastOperation(): Operation | null {
        return null;
    },
    isLoading(): boolean {
        return false;
    },
};

const noteActions: NoteActions = {
    retrieve: voidPromise,
    handleError: voidMethod,
};

const noteMutations: NoteMutations = {
    setRequested: voidMethod,
    setNotes: voidMethod,
    addNote: voidMethod,
    updateNote: voidMethod,
    deleteNote: voidMethod,
    noteError: voidMethod,
};

const noteStub: NoteModule = {
    namespaced: true,
    state: noteState,
    getters: noteGetters,
    actions: noteActions,
    mutations: noteMutations,
};

export default noteStub;
