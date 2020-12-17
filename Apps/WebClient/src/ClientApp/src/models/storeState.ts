import AuthenticationData from "@/models/authenticationData";
import { ExternalConfiguration } from "@/models/configData";
import User from "@/models/user";
import MedicationResult from "@/models/medicationResult";
import { LaboratoryOrder } from "@/models//laboratory";
import BannerError from "@/models/bannerError";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { DateWrapper } from "@/models/dateWrapper";

import { UserComment } from "@/models/userComment";
import { Dictionary } from "@/models/baseTypes";
import ImmunizationModel from "@/models/immunizationModel";

export enum StateType {
    NONE,
    INITIALIZED,
    REQUESTED,
    DEFERRED,
    ERROR,
}

export interface RootState {
    version: string;
}

export interface AuthState {
    authentication: AuthenticationData;
    isAuthenticated: boolean;
    statusMessage: string;
    error: unknown;
    stateType: StateType;
}

export interface ConfigState {
    config: ExternalConfiguration;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface UserState {
    user: User;
    smsResendDateTime?: DateWrapper;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface MedicationState {
    medicationStatements: MedicationStatementHistory[];
    medications: MedicationResult[];
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface LaboratoryState {
    laboratoryOrders: LaboratoryOrder[];
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface ImmunizationState {
    immunizations: ImmunizationModel[];
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface CommentState {
    profileComments: Dictionary<UserComment[]>;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface SidebarState {
    isOpen: boolean;
}

export interface ErrorBannerState {
    isShowing: boolean;
    errors: BannerError[];
}
