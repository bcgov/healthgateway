import { User as OidcUser } from "oidc-client";
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import UserProfile from "@/models/userProfile";
import { RootState } from "@/store/types";

export interface UserState {
    user: User;
    patientData: PatientData;
    smsResendDateTime?: DateWrapper;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface UserGetters extends GetterTree<UserState, RootState> {
    user(state: UserState): User;
    userIsRegistered(state: UserState): boolean;
    userIsActive(state: UserState): boolean;
    smsResendDateTime(state: UserState): DateWrapper | undefined;
    getPreference: (
        state: UserState
    ) => (preferenceName: string) => UserPreference | undefined;
    patientData(state: UserState): PatientData;
    isLoading(state: UserState): boolean;
}

type StoreContext = ActionContext<UserState, RootState>;
export interface UserActions extends ActionTree<UserState, RootState> {
    checkRegistration(context: StoreContext): Promise<boolean>;
    updateUserEmail(
        context: StoreContext,
        params: { emailAddress: string }
    ): Promise<void>;
    updateSMSResendDateTime(
        context: StoreContext,
        params: { dateTime: DateWrapper }
    ): void;
    updateUserPreference(
        context: StoreContext,
        params: { userPreference: UserPreference }
    ): Promise<void>;
    createUserPreference(
        context: StoreContext,
        params: { userPreference: UserPreference }
    ): Promise<void>;
    closeUserAccount(context: StoreContext): Promise<void>;
    recoverUserAccount(context: StoreContext): Promise<void>;
    retrievePatientData(context: StoreContext): Promise<void>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface UserMutation extends MutationTree<UserState> {
    setOidcUserData(state: UserState, oidcUser: OidcUser): void;
    setProfileUserData(state: UserState, userProfile: UserProfile): void;
    setSMSResendDateTime(state: UserState, dateTime: DateWrapper): void;
    setUserPreference(state: UserState, userPreference: UserPreference): void;
    setPatientData(state: UserState, patientData: PatientData): void;
    clearUserData(state: UserState): void;
    userError(state: UserState, errorMessage: string): void;
}

export interface UserModule extends Module<UserState, RootState> {
    namespaced: boolean;
    state: UserState;
    getters: UserGetters;
    actions: UserActions;
    mutations: UserMutation;
}
