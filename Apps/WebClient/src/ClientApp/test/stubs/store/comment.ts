import { voidPromise, voidMethod } from "@test/stubs/util";

import { Dictionary } from "@/models/baseTypes";
import { LoadStatus } from "@/models/storeOperations";
import { UserComment } from "@/models/userComment";
import {
    CommentActions,
    CommentGetters,
    CommentModule,
    CommentMutations,
    CommentState,
} from "@/store/modules/comment/types";

const commentState: CommentState = {
    profileComments: {},
    statusMessage: "",
    status: LoadStatus.NONE,
};

const commentGetters: CommentGetters = {
    comments(): Dictionary<UserComment[]> {
        return {};
    },
    getEntryComments: () => (): UserComment[] | null => {
        return null;
    },
    entryHasComments: () => (): boolean => {
        return false;
    },
    isLoading(): boolean {
        return false;
    },
};

const commentActions: CommentActions = {
    retrieve: voidPromise,
    createComment: voidPromise,
    updateComment: voidPromise,
    deleteComment: voidPromise,
    handleError: voidMethod,
};

const commentMutations: CommentMutations = {
    setRequested: voidMethod,
    setProfileComments: voidMethod,
    addComment: voidMethod,
    updateComment: voidMethod,
    deleteComment: voidMethod,
    commentError: voidMethod,
};

const commentStub: CommentModule = {
    namespaced: true,
    state: commentState,
    getters: commentGetters,
    actions: commentActions,
    mutations: commentMutations,
};

export default commentStub;
