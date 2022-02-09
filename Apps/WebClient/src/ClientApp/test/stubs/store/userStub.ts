import { voidMethod, voidPromise } from "@test/stubs/util";

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

const userState: UserState = {
    statusMessage: "",
    user: new User(),
    patientData: new PatientData(),
    error: false,
    status: LoadStatus.NONE,
};

const userActions: UserActions = {
    checkRegistration: voidPromise,
    updateUserEmail: voidPromise,
    updateSMSResendDateTime: voidMethod,
    updateUserPreference: voidPromise,
    createUserPreference: voidPromise,
    closeUserAccount: voidPromise,
    recoverUserAccount: voidPromise,
    retrievePatientData: voidPromise,
    handleError: voidMethod,
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
    isLoading(): boolean {
        return false;
    },
};

const userMutations: UserMutation = {
    setOidcUserData: voidMethod,
    setProfileUserData: voidMethod,
    setSMSResendDateTime: voidMethod,
    setUserPreference: voidMethod,
    setPatientData: voidMethod,
    clearUserData: voidMethod,
    userError: voidMethod,
};

const userStub: UserModule = {
    namespaced: true,
    state: userState,
    getters: userGetters,
    actions: userActions,
    mutations: userMutations,
};

export default userStub;
