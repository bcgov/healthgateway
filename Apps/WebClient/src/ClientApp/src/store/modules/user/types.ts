import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import PatientData from "@/models/patientData";
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
    patientData: PatientData;
    patientRetrievalFailed: boolean;
    smsResendDateTime?: DateWrapper;
    seenTutorialComment: boolean;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface UserGetters extends GetterTree<UserState, RootState> {
    user(state: UserState): User;
    oidcUserInfo(state: UserState): OidcUserInfo | undefined;
    userIsRegistered(state: UserState): boolean;
    userIsActive(state: UserState): boolean;
    smsResendDateTime(state: UserState): DateWrapper | undefined;
    seenTutorialComment(state: UserState): boolean;
    hasTermsOfServiceUpdated(state: UserState): boolean;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    quickLinks(state: UserState, getters: any): QuickLink[] | undefined;
    patientData(state: UserState): PatientData;
    patientRetrievalFailed(state: UserState): boolean;
    isLoading(state: UserState): boolean;
    userIsLoggedInAndActive(
        _state: UserState,
        // eslint-disable-next-line
        getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): boolean;
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
    setSeenTutorialComment(
        context: StoreContext,
        params: { value: boolean }
    ): void;
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
    setPatientData(state: UserState, patientData: PatientData): void;
    setPatientRetrievalFailed(state: UserState): void;
    clearUserData(state: UserState): void;
    setSeenTutorialComment(state: UserState, value: boolean): void;
    userError(state: UserState, errorMessage: string): void;
}

export interface UserModule extends Module<UserState, RootState> {
    namespaced: boolean;
    state: UserState;
    getters: UserGetters;
    actions: UserActions;
    mutations: UserMutation;
}
