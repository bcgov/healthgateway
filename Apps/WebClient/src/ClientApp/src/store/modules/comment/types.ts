import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";
import { RootState } from "@/store/types";

export interface CommentState {
    profileComments: Dictionary<UserComment[]>;
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface CommentGetters extends GetterTree<CommentState, RootState> {
    comments(state: CommentState): Dictionary<UserComment[]>;
    getEntryComments: (
        state: CommentState
    ) => (entryId: string) => UserComment[] | null;
    entryHasComments: (state: CommentState) => (entryId: string) => boolean;
    isLoading(state: CommentState): boolean;
}

type StoreContext = ActionContext<CommentState, RootState>;
export interface CommentActions extends ActionTree<CommentState, RootState> {
    retrieve(context: StoreContext, params: { hdid: string }): Promise<void>;
    createComment(
        context: StoreContext,
        params: { hdid: string; comment: UserComment }
    ): Promise<UserComment | undefined>;
    updateComment(
        context: StoreContext,
        params: { hdid: string; comment: UserComment }
    ): Promise<UserComment>;
    deleteComment(
        context: StoreContext,
        params: { hdid: string; comment: UserComment }
    ): Promise<void>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface CommentMutations extends MutationTree<CommentState> {
    setRequested(state: CommentState): void;
    setProfileComments(
        state: CommentState,
        profileComments: Dictionary<UserComment[]>
    ): void;
    addComment(state: CommentState, userComment: UserComment): void;
    updateComment(state: CommentState, userComment: UserComment): void;
    deleteComment(state: CommentState, userComment: UserComment): void;
    commentError(state: CommentState, error: ResultError): void;
}

export interface CommentModule extends Module<CommentState, RootState> {
    state: CommentState;
    getters: CommentGetters;
    actions: CommentActions;
    mutations: CommentMutations;
}
