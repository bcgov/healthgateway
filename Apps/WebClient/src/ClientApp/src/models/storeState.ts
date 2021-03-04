import PatientData from "@/model/patient";
import { LaboratoryOrder } from "@/models//laboratory";
import AuthenticationData from "@/models/authenticationData";
import BannerError from "@/models/bannerError";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import MedicationResult from "@/models/medicationResult";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";

import Encounter from "./encounter";
import { ResultError } from "./requestResult";
import TimelineFilter from "./timelineFilter";
import UserNote from "./userNote";

export enum LoadStatus {
    NONE,
    REQUESTED,
    ASYNC_REQUESTED,
    LOADED,
    DEFERRED,
    PROTECTED,
    ERROR,
}

export enum OperationType {
    ADD,
    UPDATE,
    DELETE,
}

export class Operation {
    public readonly id: string;
    public readonly operationType: OperationType;
    public readonly date: DateWrapper;
    constructor(id: string, operationType: OperationType) {
        this.id = id;
        this.operationType = operationType;
        this.date = new DateWrapper();
    }
}

export interface RootState {
    version: string;
    isMobile: boolean;
}

export interface AuthState {
    authentication: AuthenticationData;
    isAuthenticated: boolean;
    statusMessage: string;
    error: unknown;
    status: LoadStatus;
}

export interface ConfigState {
    config: ExternalConfiguration;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface UserState {
    user: User;
    smsResendDateTime?: DateWrapper;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface PatientState {
    patientData: PatientData;
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface MedicationState {
    medicationStatements: MedicationStatementHistory[];
    protectiveWordAttempts: number;
    medications: MedicationResult[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface LaboratoryState {
    laboratoryOrders: LaboratoryOrder[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface ImmunizationState {
    immunizations: ImmunizationEvent[];
    recommendations: Recommendation[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface EncounterState {
    patientEncounters: Encounter[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface CommentState {
    profileComments: Dictionary<UserComment[]>;
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface NoteState {
    notes: UserNote[];
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
    lastOperation: Operation | null;
}

export interface TimelineState {
    filter: TimelineFilter;
    keyword: string;
    isLinearView: boolean;
    linearDate: DateWrapper;
    calendarDate: DateWrapper;
    selectedDate: DateWrapper | null;
}

export interface NavbarState {
    isSidebarOpen: boolean;
    isHeaderShown: boolean;
}

export interface IdleState {
    isVisible: boolean;
}

export interface ErrorBannerState {
    isShowing: boolean;
    errors: BannerError[];
}
