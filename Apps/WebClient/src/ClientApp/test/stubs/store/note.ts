import RequestResult from "@/models/requestResult";
import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import {
    NoteActions,
    NoteGetters,
    NoteModule,
    NoteMutations,
    NoteState,
} from "@/store/modules/note/types";

var noteState: NoteState = {
    notes: [],
    statusMessage: "",
    status: LoadStatus.LOADED,
    lastOperation: null,
};

var noteGetters: NoteGetters = {
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

var noteActions: NoteActions = {
    retrieve(): Promise<RequestResult<UserNote[]>> {
        return new Promise(() => {});
    },
    handleError(): void {},
};

var noteMutations: NoteMutations = {
    setRequested(): void {},
    setNotes(): void {},
    addNote(): void {},
    updateNote(): void {},
    deleteNote(): void {},
    noteError(): void {},
};

var noteStub: NoteModule = {
    namespaced: true,
    state: noteState,
    getters: noteGetters,
    actions: noteActions,
    mutations: noteMutations,
};

export default noteStub;
