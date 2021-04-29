import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import {
    NoteActions,
    NoteGetters,
    NoteModule,
    NoteMutations,
    NoteState,
} from "@/store/modules/note/types";

import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

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
    retrieve: stubbedPromise,
    handleError: stubbedVoid,
};

const noteMutations: NoteMutations = {
    setRequested: stubbedVoid,
    setNotes: stubbedVoid,
    addNote: stubbedVoid,
    updateNote: stubbedVoid,
    deleteNote: stubbedVoid,
    noteError: stubbedVoid,
};

const noteStub: NoteModule = {
    namespaced: true,
    state: noteState,
    getters: noteGetters,
    actions: noteActions,
    mutations: noteMutations,
};

export default noteStub;
