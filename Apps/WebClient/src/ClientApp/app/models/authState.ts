export interface RootState {
    version: string;
}

export interface Authentication {
    token: string
}

export interface AuthState {
    authentication?: Authentication,
    statusMessage: string,
    error: boolean,
    requestedRoute: string
}