import AuthenticationData from '@/models/authenticationData'

export interface RootState {
    version: string;
}

export interface AuthState {
    authentication?: AuthenticationData,
    statusMessage: string,
    error: boolean,
    requestedRoute: string
}