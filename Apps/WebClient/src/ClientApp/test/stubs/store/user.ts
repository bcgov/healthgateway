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

var userState: UserState = {
    statusMessage: "",
    user: new User(),
    patientData: new PatientData(),
    error: false,
    status: LoadStatus.NONE,
};

var userActions: UserActions = {
    checkRegistration(): Promise<boolean> {
        return new Promise(() => {});
    },
    updateUserEmail(): Promise<void> {
        return new Promise(() => {});
    },
    updateSMSResendDateTime(): void {},
    updateUserPreference(): Promise<void> {
        return new Promise(() => {});
    },
    createUserPreference(): Promise<void> {
        return new Promise(() => {});
    },
    closeUserAccount(): Promise<void> {
        return new Promise(() => {});
    },
    recoverUserAccount(): Promise<void> {
        return new Promise(() => {});
    },
    getPatientData(): Promise<void> {
        return new Promise(() => {});
    },
    handleError() {},
};

var userGetters: UserGetters = {
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

var userMutations: UserMutation = {
    setOidcUserData() {},
    setProfileUserData() {},
    setSMSResendDateTime() {},
    setUserPreference() {},
    setPatientData() {},
    clearUserData() {},
    userError() {},
};

var userStub: UserModule = {
    namespaced: true,
    state: userState,
    getters: userGetters,
    actions: userActions,
    mutations: userMutations,
};

export default userStub;
