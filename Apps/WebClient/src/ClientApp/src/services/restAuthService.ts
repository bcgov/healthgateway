import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IAuthenticationService,
    IHttpDelegate,
} from "@/services/interfaces";
import { User as OidcUser, UserManagerSettings } from "oidc-client";
// These imports should be optimized
import { UserManager, WebStorageStateStore } from "oidc-client";

import { injectable } from "inversify";
import { OpenIdConnectConfiguration } from "@/models/configData";
import { CookieStorage } from "cookie-storage";
import { FragmentedStorage } from "@/utility/fragmentStorage";

@injectable()
export class RestAuthenticationService implements IAuthenticationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly USER_INFO_PATH: string =
        "/protocol/openid-connect/userinfo";

    private oidcUserManager!: UserManager;
    private authorityUri = "";
    private http!: IHttpDelegate;

    public initialize(
        config: OpenIdConnectConfiguration,
        httpDelegate: IHttpDelegate
    ): void {
        const oidcConfig = {
            userStore: new WebStorageStateStore({
                store: new FragmentedStorage(
                    new CookieStorage({
                        domain: null,
                        expires: null,
                        path: "/",
                        secure: false,
                        sameSite: "Strict",
                    }),
                    2000
                ),
            }),
            stateStore: new WebStorageStateStore({
                store: window.localStorage,
            }),
            authority: config.authority,
            client_id: config.clientId,
            redirect_uri: config.callbacks["Logon"],
            response_type: config.responseType,
            scope: config.scope,
            post_logout_redirect_uri: config.callbacks["Logout"],
            filterProtocolClaims: true,
            loadUserInfo: false,
            automaticSilentRenew: true,
        };
        this.logger.debug(`oidc configuration: ${JSON.stringify(oidcConfig)}`);
        this.http = httpDelegate;
        this.authorityUri = config.authority;
        this.oidcUserManager = new UserManager(oidcConfig);
        let seft = this;
        this.oidcUserManager.events.addAccessTokenExpiring(function () {
            seft.logger.debug("Token expiring...");
        });
    }

    public getOidcConfig(): UserManagerSettings {
        return this.oidcUserManager.settings;
    }

    public getUser(): Promise<OidcUser | null> {
        return new Promise<OidcUser | null>((resolve) => {
            this.oidcUserManager
                .getUser()
                .then((user) => {
                    resolve(user);
                })
                .catch((err) => {
                    this.logger.error(err);
                    resolve(null);
                });
        });
    }

    public getOidcUserProfile(): Promise<OidcUser | null> {
        return this.http.get<any>(`${this.authorityUri}${this.USER_INFO_PATH}`);
    }

    public logout(): Promise<void> {
        return this.oidcUserManager.signoutRedirect();
    }

    public signinSilent(): Promise<OidcUser> {
        return this.oidcUserManager.signinSilent();
    }

    public signinRedirect(
        idpHint: string,
        redirectPath: string
    ): Promise<void> {
        sessionStorage.setItem("vuex_oidc_active_route", redirectPath);
        return this.oidcUserManager.signinRedirect({
            extraQueryParams: {
                kc_idp_hint: idpHint,
            },
        });
    }

    public signinRedirectCallback(): Promise<OidcUser> {
        return this.oidcUserManager.signinRedirectCallback();
    }

    public removeUser(): Promise<void> {
        return this.oidcUserManager!.removeUser();
    }

    public storeUser(user: OidcUser): Promise<void> {
        return this.oidcUserManager.storeUser(user);
    }

    public clearStaleState(): Promise<void> {
        return this.oidcUserManager.clearStaleState();
    }

    public checkOidcUserSize(user: OidcUser): number {
        const key = `user:${this.oidcUserManager.settings.authority}:${this.oidcUserManager.settings.client_id}`;
        const completString = key + "=" + user.toStorageString();
        return this.stringbyteCount(completString);
    }

    //TODO: Do we still need this?
    public expireSiteMinderCookie() {
        // This expires the siteminder cookie preventing the app from login in using the cache.
        const d = new Date();
        document.cookie = `SMSESSION=;domain=.gov.bc.ca;path=/;expires=${d.toUTCString()}`;
    }

    private stringbyteCount(s: string): number {
        return encodeURIComponent("" + s).length;
    }
}
