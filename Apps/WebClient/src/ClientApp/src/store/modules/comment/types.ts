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
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}

export interface CommentGetters extends GetterTree<CommentState, RootState> {
    comments(state: CommentState): Dictionary<UserComment[]>;
    getEntryComments: (
        state: CommentState
    ) => (entryId: string) => UserComment[] | null;
    entryHasComments: (state: CommentState) => (entryId: string) => boolean;
    commentsAreLoading(state: CommentState): boolean;
}

type StoreContext = ActionContext<CommentState, RootState>;
export interface CommentActions extends ActionTree<CommentState, RootState> {
    retrieveComments(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<void>;
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
    setCommentsRequested(state: CommentState): void;
    setComments(
        state: CommentState,
        profileComments: Dictionary<UserComment[]>
    ): void;
    addComment(state: CommentState, userComment: UserComment): void;
    updateComment(state: CommentState, userComment: UserComment): void;
    deleteComment(state: CommentState, userComment: UserComment): void;
    setCommentsError(state: CommentState, error: ResultError): void;
}

export interface CommentModule extends Module<CommentState, RootState> {
    state: CommentState;
    getters: CommentGetters;
    actions: CommentActions;
    mutations: CommentMutations;
}
