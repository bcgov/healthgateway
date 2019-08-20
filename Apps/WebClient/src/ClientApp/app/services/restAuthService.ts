import { IAuthenticationService, IConfigService } from "@/services/interfaces";
import container from "@/inversify.config";
import {
  UserManager,
  WebStorageStateStore,
  User as OidcUser,
  UserManagerSettings
} from "oidc-client";

import { injectable } from "inversify";
import "reflect-metadata";
import store from "@/store/store";
import { OpenIdConnectConfiguration } from "@/models/ConfigData";

@injectable()
export class RestAuthenticationService implements IAuthenticationService {
  private _oidcUserManager!: UserManager;
  private oidcUserManager = (): UserManager => {
    if (!this._oidcUserManager) {
      const config = this.getOidcConfig();
      console.log("oidc configuration: ", config);
      this._oidcUserManager = new UserManager(config);
    }
    return this._oidcUserManager;
  };

  constructor() {}

  public getOidcConfig(): UserManagerSettings {
    console.log("getting oidc config...");
    const config: OpenIdConnectConfiguration =
      store.getters["config/openIdConnect"];
    console.log(config);
    return {
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
  }

  public getUser(): Promise<OidcUser | null> {
    return new Promise<OidcUser | null>(resolve => {
      this.oidcUserManager()
        .getUser()
        .then(user => {
          resolve(user);
        })
        .catch(() => {
          resolve(null);
        });
    });
  }

  public logout(): Promise<void> {
    return this.oidcUserManager().signoutRedirect();
  }

  public signinSilent(): Promise<OidcUser> {
    return this.oidcUserManager().signinSilent();
  }

  public signinRedirect(idpHint: string, redirectPath: string): Promise<void> {
    var fullRedirectUrl = new URL(redirectPath, window.location.href);
    console.log(fullRedirectUrl);
    console.log(redirectPath);
    sessionStorage.setItem("vuex_oidc_active_route", redirectPath);
    return this.oidcUserManager().signinRedirect({
      extraQueryParams: {
        kc_idp_hint: idpHint
      }
    });
  }
  public signinRedirectCallback(): Promise<OidcUser> {
    return this.oidcUserManager().signinRedirectCallback();
  }

  //TODO: Do we still need this?
  public expireSiteMinderCookie() {
    // This expires the siteminder cookie preventing the app from login in using the cache.
    var d = new Date();
    document.cookie = `SMSESSION=;domain=.gov.bc.ca;path=/;expires=${d.toUTCString()}`;
  }
}
