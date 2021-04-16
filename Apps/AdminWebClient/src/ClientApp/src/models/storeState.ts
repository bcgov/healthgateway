import AuthenticationData from "@/models/authenticationData";
import ExternalConfiguration from "@/models/externalConfiguration";

export const enum StateType {
    NONE,
    INITIALIZED,
    REQUESTED,
    ERROR,
}

export interface RootState {
    version: string;
}

export interface DrawerState {
    isOpen: boolean;
}

export interface AuthState {
    authentication?: AuthenticationData;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}

export interface ConfigState {
    config: ExternalConfiguration;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}
