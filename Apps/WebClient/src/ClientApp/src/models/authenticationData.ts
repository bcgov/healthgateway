export default interface AuthenticationData {
    accessToken?: string;
    idToken?: string;
    isChecked: boolean;
    scopes?: string[];
    error?: string;
    identityProvider: string;
}
