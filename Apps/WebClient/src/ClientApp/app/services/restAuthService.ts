import { IAuthenticationService, IConfigService } from "@/services/interfaces";
import {
  UserManager,
  WebStorageStateStore,
  User as OidcUser,
  UserManagerSettings
} from "oidc-client";

import { injectable } from "inversify";
import "reflect-metadata";
import { OpenIdConnectConfiguration } from "@/models/ConfigData";

@injectable()
export class RestAuthenticationService implements IAuthenticationService {
  private oidcUserManager?: UserManager;

  constructor() {}

  public initialize(config: OpenIdConnectConfiguration): void {
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
    this.oidcUserManager = new UserManager(oidcConfig);
  }

  public getOidcConfig(): UserManagerSettings {
    return this.oidcUserManager!.settings;
  }

  public getUser(): Promise<OidcUser | null> {
    return new Promise<OidcUser | null>(resolve => {
      this.oidcUserManager!.getUser()
        .then(user => {
          resolve(user);
        })
        .catch(() => {
          resolve(null);
        });
    });
  }

  public logout(): Promise<void> {
    return this.oidcUserManager!.signoutRedirect();
  }

  public signinSilent(): Promise<OidcUser> {
    return this.oidcUserManager!.signinSilent();
  }

  public signinRedirect(idpHint: string, redirectPath: string): Promise<void> {
    var fullRedirectUrl = new URL(redirectPath, window.location.href);
    console.log(fullRedirectUrl);
    console.log(redirectPath);
    sessionStorage.setItem("vuex_oidc_active_route", redirectPath);
    return this.oidcUserManager!.signinRedirect({
      extraQueryParams: {
        kc_idp_hint: idpHint
      }
    });
  }
  public signinRedirectCallback(): Promise<OidcUser> {
    return this.oidcUserManager!.signinRedirectCallback();
  }

  //TODO: Do we still need this?
  public expireSiteMinderCookie() {
    // This expires the siteminder cookie preventing the app from login in using the cache.
    var d = new Date();
    document.cookie = `SMSESSION=;domain=.gov.bc.ca;path=/;expires=${d.toUTCString()}`;
  }
}
