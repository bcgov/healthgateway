import { voidMethod, voidPromise } from "@test/stubs/util";

import { LoadStatus } from "@/models/storeOperations";
import { WalletConnection, WalletCredential } from "@/models/wallet";
import {
    CredentialActions,
    CredentialGetters,
    CredentialModule,
    CredentialMutation,
    CredentialState,
} from "@/store/modules/credential/types";

const credentialState: CredentialState = {
    statusMessage: "",
    error: false,
    status: LoadStatus.NONE,
};

const credentialActions: CredentialActions = {
    createConnection: voidPromise,
    retrieveConnection: voidPromise,
    createCredential: voidPromise,
    revokeCredential: voidPromise,
    handleError: voidPromise,
};

const credentialGetters: CredentialGetters = {
    connection(): WalletConnection | undefined {
        return undefined;
    },
    credentials(): WalletCredential[] | null {
        return [];
    },
    isLoading(): boolean {
        return false;
    },
};

const credentialMutations: CredentialMutation = {
    setRequested: voidMethod,
    setConnection: voidMethod,
    credentialError: voidMethod,
};

const credentialStub: CredentialModule = {
    namespaced: true,
    state: credentialState,
    getters: credentialGetters,
    actions: credentialActions,
    mutations: credentialMutations,
};

export default credentialStub;
