import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import {
    NoteState,
    NoteGetters,
    NoteActions,
    NoteMutations,
    NoteModule,
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
    oidcCheckUser(): Promise<boolean> {
        return new Promise(() => {});
    },
    noteenticateOidc(): Promise<void> {
        return new Promise(() => {});
    },
    oidcSignInCallback(): Promise<string> {
        return new Promise(() => {});
    },
    noteenticateOidcSilent(): Promise<void> {
        return new Promise(() => {});
    },
    oidcWasNoteenticated(): void {},
    getOidcUser(): Promise<void> {
        return new Promise(() => {});
    },
    signOutOidc(): void {},
    signOutOidcCallback(): Promise<string> {
        return new Promise(() => {});
    },
    clearStorage(): void {},
};

const noteMutations: NoteMutations = {
    setOidcNote(): void {},
    unsetOidcNote(): void {},
    setOidcNoteIsChecked(): void {},
    setOidcError(): void {},
};

const noteStub: NoteModule = {
    namespaced: true,
    state: noteState,
    getters: noteGetters,
    actions: noteActions,
    mutations: noteMutations,
};

export default noteStub;
