import AuthenticationData from '@/models/authenticationData';

export enum StateType {
    NONE,
    INITIALIZED,
    REQUESTED,
    ERROR
}

export interface RootState {
    version: string;
}

export interface AuthState {
    authentication?: AuthenticationData;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}
