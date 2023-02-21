import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import { RootState } from "@/store/types";

export interface NoteState {
    notes: UserNote[];
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
    lastOperation: Operation | null;
}

export interface NoteGetters extends GetterTree<NoteState, RootState> {
    notes(state: NoteState): UserNote[];
    notesCount(state: NoteState): number;
    notesAreLoading(state: NoteState): boolean;
    lastOperation(state: NoteState): Operation | null;
}

type StoreContext = ActionContext<NoteState, RootState>;
export interface NoteActions extends ActionTree<NoteState, RootState> {
    retrieveNotes(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<UserNote[]>>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface NoteMutations extends MutationTree<NoteState> {
    setNotesRequested(state: NoteState): void;
    setNotes(state: NoteState, notes: UserNote[]): void;
    addNote(state: NoteState, note: UserNote): void;
    updateNote(state: NoteState, note: UserNote): void;
    deleteNote(state: NoteState, note: UserNote): void;
    setNotesError(state: NoteState, error: Error): void;
}

export interface NoteModule extends Module<NoteState, RootState> {
    namespaced: boolean;
    state: NoteState;
    getters: NoteGetters;
    actions: NoteActions;
    mutations: NoteMutations;
}
