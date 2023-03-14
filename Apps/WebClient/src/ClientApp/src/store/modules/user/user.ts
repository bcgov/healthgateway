import Patient from "@/models/patient";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { UserModule, UserState } from "./types";

const state: UserState = {
    user: new User(),
    patient: new Patient(),
    patientRetrievalFailed: false,
    seenTutorialComment: false,
    statusMessage: "",
    error: false,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const user: UserModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
