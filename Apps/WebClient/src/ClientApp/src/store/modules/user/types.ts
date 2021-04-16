import { User as OidcUser } from "oidc-client";
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

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
}

type UserContext = ActionContext<UserState, RootState>;
export interface UserActions extends ActionTree<UserState, RootState> {
    checkRegistration(context: UserContext): Promise<boolean>;
    updateUserEmail(
        context: UserContext,
        params: { emailAddress: string }
    ): Promise<void>;
    updateSMSResendDateTime(
        context: UserContext,
        params: { dateTime: DateWrapper }
    ): void;
    updateUserPreference(
        context: UserContext,
        params: { userPreference: UserPreference }
    ): Promise<void>;
    createUserPreference(
        context: UserContext,
        params: { userPreference: UserPreference }
    ): Promise<void>;
    closeUserAccount(context: UserContext): Promise<void>;
    recoverUserAccount(context: UserContext): Promise<void>;
    getPatientData(context: UserContext): Promise<void>;
    handleError(context: UserContext, error: ResultError): void;
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
