import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

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
    retrieve: stubbedPromise,
    createComment: stubbedPromise,
    updateComment: stubbedPromise,
    deleteComment: stubbedPromise,
    handleError: stubbedVoid,
};

const commentMutations: CommentMutations = {
    setRequested: stubbedVoid,
    setProfileComments: stubbedVoid,
    addComment: stubbedVoid,
    updateComment: stubbedVoid,
    deleteComment: stubbedVoid,
    commentError: stubbedVoid,
};

const commentStub: CommentModule = {
    namespaced: true,
    state: commentState,
    getters: commentGetters,
    actions: commentActions,
    mutations: commentMutations,
};

export default commentStub;
