import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import {
    UserActions,
    UserGetters,
    UserModule,
    UserMutation,
    UserState,
} from "@/store/modules/user/types";

import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

const userState: UserState = {
    statusMessage: "",
    user: new User(),
    patientData: new PatientData(),
    error: false,
    status: LoadStatus.NONE,
};

const userActions: UserActions = {
    checkRegistration: stubbedPromise,
    updateUserEmail: stubbedPromise,
    updateSMSResendDateTime: stubbedVoid,
    updateUserPreference: stubbedPromise,
    createUserPreference: stubbedPromise,
    closeUserAccount: stubbedPromise,
    recoverUserAccount: stubbedPromise,
    getPatientData: stubbedPromise,
    handleError: stubbedVoid,
};

const userGetters: UserGetters = {
    user: (): User => {
        return new User();
    },
    userIsRegistered(): boolean {
        return true;
    },
    userIsActive(): boolean {
        return true;
    },
    smsResendDateTime(): DateWrapper | undefined {
        return undefined;
    },
    getPreference: () => (): UserPreference | undefined => {
        return undefined;
    },
    patientData(): PatientData {
        return new PatientData();
    },
};

const userMutations: UserMutation = {
    setOidcUserData: stubbedVoid,
    setProfileUserData: stubbedVoid,
    setSMSResendDateTime: stubbedVoid,
    setUserPreference: stubbedVoid,
    setPatientData: stubbedVoid,
    clearUserData: stubbedVoid,
    userError: stubbedVoid,
};

const userStub: UserModule = {
    namespaced: true,
    state: userState,
    getters: userGetters,
    actions: userActions,
    mutations: userMutations,
};

export default userStub;
