import {
  IAuthenticationService,
  IHttpDelegate
} from "@/services/interfaces";
import {
  UserManager,
  WebStorageStateStore,
  User as OidcUser,
  UserManagerSettings
} from "oidc-client";

import { injectable } from "inversify";
import { OpenIdConnectConfiguration } from "@/models/configData";

@injectable()
export class RestAuthenticationService implements IAuthenticationService {
  private readonly AUTHORITY_BASE_URI: string =
    "/protocol/openid-connect/userinfo";

  private oidcUserManager!: UserManager;
  private authorityUri = "";
  private http!: IHttpDelegate;

  public initialize(
    config: OpenIdConnectConfiguration,
    httpDelegate: IHttpDelegate
  ): void {
    const oidcConfig = {
      userStore: new WebStorageStateStore({ store: window.localStorage }),
      authority: config.authority,
      client_id: config.clientId,
      redirect_uri: config.callbacks["Logon"],
      response_type: config.responseType,
      scope: config.scope,
      post_logout_redirect_uri: config.callbacks["Logout"],
      filterProtocolClaims: true,
      loadUserInfo: false
    };
    console.log("oidc configuration: ", oidcConfig);
    this.http = httpDelegate;
    this.authorityUri = config.authority;
    this.oidcUserManager = new UserManager(oidcConfig);
  }

  public getOidcConfig(): UserManagerSettings {
    return this.oidcUserManager.settings;
  }

  public getUser(): Promise<OidcUser | null> {
    return new Promise<OidcUser | null>(resolve => {
      this.oidcUserManager
        .getUser()
        .then(user => {
          resolve(user);
        })
        .catch(() => {
          resolve(null);
        });
    });
  }

  public getUserProfile(): Promise<OidcUser | null> {
    return this.http.get<any>(`${this.authorityUri}${this.AUTHORITY_BASE_URI}`);
  }

  public logout(): Promise<void> {
    return this.oidcUserManager.signoutRedirect();
  }

  public signinSilent(): Promise<OidcUser> {
    return this.oidcUserManager.signinSilent();
  }

  public signinRedirect(idpHint: string, redirectPath: string): Promise<void> {
    var fullRedirectUrl = new URL(redirectPath, window.location.href);
    sessionStorage.setItem("vuex_oidc_active_route", redirectPath);
    return this.oidcUserManager.signinRedirect({
      extraQueryParams: {
        kc_idp_hint: idpHint
      }
    });
  }

  public signinRedirectCallback(): Promise<OidcUser> {
    return this.oidcUserManager.signinRedirectCallback();
  }

  public removeUser(): Promise<void> {
    return this.oidcUserManager!.removeUser();
  }

  public clearStaleState(): Promise<void> {
    return this.oidcUserManager.clearStaleState();
  }

  //TODO: Do we still need this?
  public expireSiteMinderCookie() {
    // This expires the siteminder cookie preventing the app from login in using the cache.
    var d = new Date();
    document.cookie = `SMSESSION=;domain=.gov.bc.ca;path=/;expires=${d.toUTCString()}`;
  }
}
