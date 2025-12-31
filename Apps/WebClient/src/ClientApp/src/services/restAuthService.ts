import Keycloak, { KeycloakConfig } from "keycloak-js";

import { OpenIdConnectConfiguration } from "@/models/configData";
import { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import { IAuthenticationService, ILogger } from "@/services/interfaces";

/** The number of seconds between initiation of a token refresh and expiry of the old token. */
const REFRESH_CUSHION = 30;

export class RestAuthenticationService implements IAuthenticationService {
    private readonly logger;
    private readonly keycloak;
    private readonly scope!: string;
    private readonly logonCallback!: string;
    private readonly logoutCallback!: string;

    // RestAuthenticationService.GetService() should be called instead of using the constructor directly.
    constructor(
        logger: ILogger,
        keycloak: Keycloak,
        oidcConfig: OpenIdConnectConfiguration
    ) {
        this.logger = logger;
        this.keycloak = keycloak;

        this.scope = oidcConfig.scope;
        this.logonCallback =
            oidcConfig.callbacks?.["Logon"] ||
            `${location.origin}/loginCallback`;
        this.logoutCallback =
            oidcConfig.callbacks?.["Logout"] ||
            `${location.origin}/logoutComplete`;
    }

    public static async GetService(
        logger: ILogger,
        oidcConfig: OpenIdConnectConfiguration
    ): Promise<IAuthenticationService> {
        const [url, realm] = oidcConfig.authority.split("/realms/");
        const keycloakConfig: KeycloakConfig = {
            url,
            realm,
            clientId: oidcConfig.clientId,
        };
        const keycloak = new Keycloak(keycloakConfig);

        keycloak.onReady = () => logger.verbose("Keycloak: onReady");
        keycloak.onAuthSuccess = () =>
            logger.verbose("Keycloak: onAuthSuccess");
        keycloak.onAuthError = (errorData) => {
            logger.verbose(`Keycloak: onAuthError - ${errorData?.error}`);
            logger.error(errorData?.error_description || "Unknown error");
        };
        keycloak.onAuthRefreshSuccess = () =>
            logger.verbose("Keycloak: onAuthRefreshSuccess");
        keycloak.onAuthRefreshError = () =>
            logger.verbose("Keycloak: onAuthRefreshError");
        keycloak.onAuthLogout = () => logger.verbose("Keycloak: onAuthLogout");
        keycloak.onTokenExpired = () =>
            logger.verbose("Keycloak: onTokenExpired");
        keycloak.onActionUpdate = (status: string) =>
            logger.verbose(`Keycloak: onActionUpdate - ${status}`);

        await keycloak.init({
            onLoad: "check-sso",
            scope: oidcConfig.scope,
        });

        return new RestAuthenticationService(logger, keycloak, oidcConfig);
    }

    public async signIn(
        redirectPath: string,
        idpHint?: string
    ): Promise<OidcTokenDetails> {
        this.logger.verbose("Checking authentication...");
        const tokenDetails = this.getOidcTokenDetails();
        if (tokenDetails !== null) {
            this.logger.verbose("Already authenticated");
            return tokenDetails;
        }

        this.logger.verbose(
            "Not yet authenticated; redirecting to Keycloak login..."
        );
        const escapedRedirectPath = encodeURI(redirectPath);
        const callbackUri = `${this.logonCallback}?redirect=${escapedRedirectPath}`;

        await this.keycloak.login({
            scope: this.scope,
            redirectUri: callbackUri,
            idpHint,
        });

        // if keycloak.login() doesn't cause a redirect, something is terribly wrong
        throw Error("Redirect to Keycloak login page failed");
    }

    public signOut(): Promise<void> {
        return this.keycloak.logout({
            redirectUri: this.logoutCallback,
        });
    }

    public refreshToken(): Promise<boolean> {
        return this.keycloak.updateToken(REFRESH_CUSHION + 5);
    }

    public getOidcTokenDetails(): OidcTokenDetails | null {
        if (!this.keycloak?.authenticated) {
            return null;
        }

        const currentTime = Math.ceil(new Date().getTime() / 1000);
        const tokenExpiryTime = this.keycloak.tokenParsed?.exp ?? currentTime;
        const timeSkew = this.keycloak.timeSkew ?? 0;

        return {
            idToken: this.keycloak.idToken ?? "",
            sessionState: this.keycloak.sessionId,
            refreshToken: this.keycloak.refreshToken,
            accessToken: this.keycloak.token ?? "",
            expired: this.keycloak.isTokenExpired(),
            refreshTokenTime: tokenExpiryTime + timeSkew - REFRESH_CUSHION,
            hdid: this.keycloak.idTokenParsed?.hdid ?? "",
        };
    }

    public async getOidcUserInfo(): Promise<OidcUserInfo> {
        if (!this.keycloak.userInfo) {
            await this.keycloak.loadUserInfo();
        }

        return this.keycloak.userInfo as OidcUserInfo;
    }

    public clearState(): void {
        this.keycloak.clearToken();
    }
}
