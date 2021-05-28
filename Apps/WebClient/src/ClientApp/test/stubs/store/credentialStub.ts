import { voidMethod, voidPromise } from "@test/stubs/util";

import { WalletConnection, WalletCredential } from "@/models/credential";
import { LoadStatus } from "@/models/storeOperations";
import {
    CredentialActions,
    CredentialGetters,
    CredentialModule,
    CredentialMutation,
    CredentialState,
} from "@/store/modules/credential/types";

const credentialState: CredentialState = {
    credentials: [],
    statusMessage: "",
    error: false,
    status: LoadStatus.NONE,
};

const credentialActions: CredentialActions = {
    retrieveConnection: voidPromise,
    retrieveCredentials: voidPromise,
    createConnection: voidPromise,
    handleError: voidPromise,
};

const credentialGetters: CredentialGetters = {
    connection(): WalletConnection | undefined {
        return undefined;
    },
    credentials(): WalletCredential[] {
        return [];
    },
};

const credentialMutations: CredentialMutation = {
    setRequested: voidMethod,
    setConnection: voidMethod,
    setCredentials: voidMethod,
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
