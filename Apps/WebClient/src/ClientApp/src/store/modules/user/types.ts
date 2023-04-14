import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import Patient from "@/models/patient";
import { QuickLink } from "@/models/quickLink";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import User, { OidcUserInfo } from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import { RootState } from "@/store/types";

export interface UserState {
    user: User;
    oidcUserInfo?: OidcUserInfo;
    patient: Patient;
    patientRetrievalFailed: boolean;
    smsResendDateTime?: DateWrapper;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface UserGetters extends GetterTree<UserState, RootState> {
    user(state: UserState): User;
    lastLoginDateTime(state: UserState): StringISODateTime | undefined;
    oidcUserInfo(state: UserState): OidcUserInfo | undefined;
    isValidIdentityProvider(state: UserState): boolean;
    userIsRegistered(state: UserState): boolean;
    userIsActive(state: UserState): boolean;
    smsResendDateTime(state: UserState): DateWrapper | undefined;
    hasTermsOfServiceUpdated(state: UserState): boolean;
    quickLinks(state: UserState): QuickLink[] | undefined;
    patient(state: UserState): Patient;
    patientRetrievalFailed(state: UserState): boolean;
    isLoading(state: UserState): boolean;
}

type StoreContext = ActionContext<UserState, RootState>;
export interface UserActions extends ActionTree<UserState, RootState> {
    createProfile(
        context: StoreContext,
        params: { request: CreateUserRequest }
    ): Promise<void>;
    retrieveProfile(context: StoreContext): Promise<void>;
    updateUserEmail(
        context: StoreContext,
        params: { emailAddress: string }
    ): Promise<void>;
    updateSMSResendDateTime(
        context: StoreContext,
        params: { dateTime: DateWrapper }
    ): void;
    setUserPreference(
        context: StoreContext,
        params: { preference: UserPreference }
    ): Promise<void>;
    updateQuickLinks(
        context: StoreContext,
        params: { hdid: string; quickLinks: QuickLink[] }
    ): Promise<void>;
    validateEmail(
        context: StoreContext,
        params: { inviteKey: string }
    ): Promise<RequestResult<boolean>>;
    closeUserAccount(context: StoreContext): Promise<void>;
    recoverUserAccount(context: StoreContext): Promise<void>;
    retrieveEssentialData(context: StoreContext): Promise<void>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface UserMutation extends MutationTree<UserState> {
    setOidcUserInfo(state: UserState, userInfo: OidcUserInfo): void;
    setProfileUserData(state: UserState, userProfile: UserProfile): void;
    setEmailVerified(state: UserState): void;
    setSMSResendDateTime(state: UserState, dateTime: DateWrapper): void;
    setUserPreference(state: UserState, userPreference: UserPreference): void;
    setPatient(state: UserState, patient: Patient): void;
    setPatientRetrievalFailed(state: UserState): void;
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
