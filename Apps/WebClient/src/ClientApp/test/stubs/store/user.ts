import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import { ConfigModule } from "@/store/modules/config/types";
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
    closeUserAccount(context): Promise<void> {
        return new Promise(() => {});
    },
    recoverUserAccount(context): Promise<void> {
        return new Promise(() => {});
    },
    getPatientData(): Promise<void> {
        return new Promise(() => {});
    },
    handleError() {},
};

const userGetters: UserGetters = {
    user: (): User => {
        return new User();
    },
    userIsRegistered(state: UserState): boolean {
        return true;
    },
    userIsActive(state: UserState): boolean {
        return true;
    },
    smsResendDateTime(state: UserState): DateWrapper | undefined {
        return undefined;
    },
    getPreference: (state: UserState) => (
        preferenceName: string
    ): UserPreference | undefined => {
        return undefined;
    },
    patientData(state: UserState): PatientData {
        return new PatientData();
    },
};

const userMutations: UserMutation = {
    setOidcUserData() {},
    setProfileUserData() {},
    setSMSResendDateTime() {},
    setUserPreference() {},
    setPatientData() {},
    clearUserData() {},
    userError() {},
};

const userStub: UserModule = {
    state: userState,
    namespaced: true,
    getters: userGetters,
    actions: userActions,
    mutations: userMutations,
};

export default userStub;
