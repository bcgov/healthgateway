import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { UserModule, UserState } from "./types";

const state: UserState = {
    statusMessage: "",
    user: new User(),
    patientData: new PatientData(),
    seenTutorialComment: false,
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
