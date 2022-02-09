import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus, Operation } from "@/models/storeOperations";
import UserNote from "@/models/userNote";
import { RootState } from "@/store/types";

export interface NoteState {
    notes: UserNote[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
    lastOperation: Operation | null;
}

export interface NoteGetters extends GetterTree<NoteState, RootState> {
    notes(state: NoteState): UserNote[];
    noteCount(state: NoteState): number;
    lastOperation(state: NoteState): Operation | null;
    isLoading(state: NoteState): boolean;
}

type StoreContext = ActionContext<NoteState, RootState>;
export interface NoteActions extends ActionTree<NoteState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<UserNote[]>>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface NoteMutations extends MutationTree<NoteState> {
    setRequested(state: NoteState): void;
    setNotes(state: NoteState, notes: UserNote[]): void;
    addNote(state: NoteState, note: UserNote): void;
    updateNote(state: NoteState, note: UserNote): void;
    deleteNote(state: NoteState, note: UserNote): void;
    noteError(state: NoteState, error: Error): void;
}

export interface NoteModule extends Module<NoteState, RootState> {
    namespaced: boolean;
    state: NoteState;
    getters: NoteGetters;
    actions: NoteActions;
    mutations: NoteMutations;
}
